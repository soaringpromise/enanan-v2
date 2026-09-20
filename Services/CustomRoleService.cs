using EnananV2.Definitions.Exceptions;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EnananV2.Services;

public sealed class CustomRoleService(DatabaseService database, ILogger<CustomRoleService> logger)
{
    public async Task CreateCustomRoleAsync(
        Guild guild, GuildUser targetUser, ulong botUserId, string title, Color color)
    {
        Role? role = null;
        try
        {
            role = await guild.CreateRoleAsync(
                new RoleProperties()
                    .WithName(title)
                    .WithColors(new RoleColorsProperties(color))
                    .WithMentionable(false));

            await guild.AddUserRoleAsync(targetUser.Id, role.Id);
            await TryMoveRoleAsync(guild, role, botUserId);

            if (!await database.GuildMember.AssignUserCustomRole(guild.Id, targetUser.Id, role.Id)) 
                throw new InvalidRequestException("Failed to save the custom role.");
        }
        catch
        {
            if (role is not null) await RollbackRoleAsync(role, guild.Id);
            throw;
        }
    }
    
    public async Task EditCustomRoleAsync(Guild guild, ulong roleId, string? newName, Color? newColor)
    {
        var role = await guild.GetRoleAsync(roleId);
        
        var nameUnchanged = string.IsNullOrWhiteSpace(newName) || role.Name == newName;
        var colorUnchanged = newColor is null || role.Colors.PrimaryColor == newColor.Value;
        if (nameUnchanged && colorUnchanged) throw new InvalidRequestException("The role already has those values.");

        await role.ModifyAsync(properties =>
        {
            if (!string.IsNullOrWhiteSpace(newName)) properties.Name = newName;
            if (newColor is not null) properties.WithColors(new RoleColorsProperties(newColor.Value));
        });
    }
    
    public async Task DeleteCustomRoleAsync(Guild guild, ulong userId, ulong roleId)
    {
        var role = await guild.GetRoleAsync(roleId);

        if (!await database.GuildMember.RemoveUserCustomRole(guild.Id, userId))
            throw new InvalidRequestException("Failed to remove the custom role from the database.");

        try
        {
            await role.DeleteAsync();
        }
        catch
        {
            try
            {
                var restored = await database.GuildMember.AssignUserCustomRole(guild.Id, userId, roleId);

                if (!restored)
                {
                    logger.LogError(
                        "Failed to restore custom role {RoleId} for user {UserId} in guild {GuildId}.",
                        roleId,
                        userId,
                        guild.Id);
                }
            }
            catch (Exception rollbackException)
            {
                logger.LogError(
                    rollbackException,
                    "Failed to restore custom role {RoleId} for user {UserId} in guild {GuildId}.",
                    roleId,
                    userId,
                    guild.Id);
            }

            throw;
        }
    }

    private static async Task TryMoveRoleAsync(Guild guild, Role role, ulong botUserId)
    {
        try
        {
            var bot = await guild.GetUserAsync(botUserId);
            var botTopRole = bot.GetRoles(guild).MaxBy(r => r.Position);
            if (botTopRole is null) return;
            var targetPosition = Math.Max(botTopRole.RawPosition - 1, 1);

            await guild.ModifyRolePositionsAsync([
                new RolePositionProperties(role.Id).WithPosition(targetPosition)
            ]);
        }
        catch (RestException) { /* Ignore failure to move. */ }
    }

    private async Task RollbackRoleAsync(Role role, ulong guildId)
    {
        try { await role.DeleteAsync(); }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to roll back role {RoleId} in guild {GuildId}.",
                role.Id, guildId);
        }
    }
}