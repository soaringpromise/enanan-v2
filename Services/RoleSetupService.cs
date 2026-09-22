using System.Net;
using EnananV2.Definitions.Data;
using EnananV2.Definitions.Exceptions;
using EnananV2.Definitions.Models;
using EnananV2.Tools.Factories;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EnananV2.Services;

public sealed class RoleSetupService(ILogger<RoleSetupService> logger, DatabaseService database)
{
    public async Task SetupRolesAsync(
        Guild guild, TextGuildChannel roleChannel, RoleMode roleMode, IReadOnlyCollection<string> roleTypes)
    {
        var createdRoleIds = new List<ulong>();
        var createdMessages = new List<RestMessage>();

        try
        {
            if (roleMode is RoleMode.ProjectSekai or RoleMode.Both)
                await CreateColoredRolesAsync(guild, roleChannel, createdRoleIds, createdMessages);

            if (roleTypes.Contains("tiering"))
            {
                await CreateGenericRolesAsync(
                    guild,
                    roleChannel,
                    PredefinedRoles.Tiering.All,
                    "tieringRoleSelect",
                    "Tiering",
                    "Choose how you usually participate in tiering, co-op, and event runs.",
                    "Choose a tiering role...",
                    Cdn.Ena("tiering"),
                    createdRoleIds,
                    createdMessages);
            }

            if (roleTypes.Contains("identity"))
            {
                await CreateGenericRolesAsync(
                    guild,
                    roleChannel,
                    PredefinedRoles.Identity.All,
                    "identityRoleSelect",
                    "Identity",
                    "Choose the roles that best describe how you'd like others to refer to you.",
                    "Choose an identity role...",
                    Cdn.Ena("identity"),
                    createdRoleIds,
                    createdMessages);
            }

            if (roleTypes.Contains("utility"))
            {
                await CreateGenericRolesAsync(
                    guild,
                    roleChannel,
                    PredefinedRoles.Utility.All,
                    "utilityRoleSelect",
                    "Utility",
                    "Opt in to the server activities and notifications you're interested in.",
                    "Choose a utility role...",
                    Cdn.Ena("utility"),
                    createdRoleIds,
                    createdMessages);
            }
        }
        catch (Exception e)
        {
            await RollbackSetupAsync(guild, createdRoleIds, createdMessages, e);
            throw;
        }
    }

    private async Task CreateColoredRolesAsync(
        Guild guild, TextGuildChannel roleChannel, List<ulong> createdRoleIds, List<RestMessage> createdMessages)
    {
        var characterRoleIds = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
        var unitRoleIds = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);

        var characters = CharacterCatalog.Characters.Values.OrderBy(x => x.Id);
        var units = CharacterCatalog.Units.Values.OrderBy(x => x.Id).ToArray();

        foreach (var character in characters)
        {
            var roleProperties = new RoleProperties()
                .WithName(character.RoleName)
                .WithColors(new RoleColorsProperties(new Color(character.Color)))
                .WithMentionable(false);

            var role = await CreateRoleSafeAsync(guild, roleProperties, createdRoleIds);
            characterRoleIds[character.Name] = role.Id;
        }

        foreach (var unit in units)
        {
            var roleProperties = new RoleProperties()
                .WithName(unit.RoleName)
                .WithColors(new RoleColorsProperties(new Color(unit.Color)))
                .WithMentionable(false);

            var role = await CreateRoleSafeAsync(guild, roleProperties, createdRoleIds);
            unitRoleIds[unit.Name] = role.Id;
        }

        foreach (var unit in units)
        {
            var message = await RoleMenuFactory.SendCharacterRolePanelAsync(roleChannel, unit, characterRoleIds);
            createdMessages.Add(message);
            await database.GuildSetup.AddMessage(guild.Id, roleChannel.Id, message.Id);
        }

        var unitMessage = await RoleMenuFactory.SendUnitRolePanelAsync(roleChannel, unitRoleIds);
        createdMessages.Add(unitMessage);
        await database.GuildSetup.AddMessage(guild.Id, roleChannel.Id, unitMessage.Id);
    }

    private async Task CreateGenericRolesAsync(
        Guild guild, TextGuildChannel roleChannel, IEnumerable<RoleProperties> roles,
        string componentId, string property, string description, string placeholder, string iconUrl,
        List<ulong> createdRoleIds, List<RestMessage> createdMessages)
    {
        var roleIds = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
        foreach (var properties in roles)
        {
            var role = await CreateRoleSafeAsync(guild, properties, createdRoleIds);
            roleIds[role.Name] = role.Id;
        }

        var message = await RoleMenuFactory.SendGenericRolePanelAsync(
            roleChannel, roleIds, componentId, property, description, placeholder, iconUrl);
        
        createdMessages.Add(message);
        await database.GuildSetup.AddMessage(guild.Id, roleChannel.Id, message.Id);
    }

    private async Task<Role> CreateRoleSafeAsync(Guild guild, RoleProperties properties, List<ulong> createdRoleIds)
    {
        Role role;

        try
        {
            role = await guild.CreateRoleAsync(properties);

            createdRoleIds.Add(role.Id);
            await database.GuildSetup.AddRole(guild.Id, role.Id);
            await database.Guild.SetStaticRoleAnchorIfUnset(guild.Id, role.Id);
        }
        catch (RestException e)
        {
            logger.LogError(e, "Failed to create role in guild {GuildId}.", guild.Id);
            throw new InvalidRequestException("There was a problem creating one or more roles.");
        }

        return role;
    }

    private async Task RollbackSetupAsync(
        Guild guild, IReadOnlyCollection<ulong> createdRoleIds, IReadOnlyCollection<RestMessage> createdMessages,
        Exception original)
    {
        var rollbackFailed = false;
        logger.LogError(
            original,
            "Setup failed for guild {GuildId} after creating {RoleCount} role(s) and {MessageCount} panel(s). Rolling back.",
            guild.Id,
            createdRoleIds.Count,
            createdMessages.Count);

        foreach (var message in createdMessages.Reverse())
        {
            try
            {
                try
                {
                    await message.DeleteAsync();
                }
                catch (RestException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning(
                        "Role panel {MessageId} was already gone during rollback.",
                        message.Id);
                }

                await database.GuildSetup.RemoveMessage(guild.Id, message.Id);
            }
            catch (Exception e)
            {
                rollbackFailed = true;

                logger.LogError(
                    e,
                    "Failed to roll back role panel {MessageId} in guild {GuildId}.",
                    message.Id,
                    guild.Id);
            }
        }

        foreach (var roleId in createdRoleIds.Reverse())
        {
            try
            {
                try
                {
                    await guild.DeleteRoleAsync(roleId);
                }
                catch (RestException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning(
                        "Role {RoleId} was already gone during rollback.",
                        roleId);
                }

                await database.GuildSetup.RemoveRole(guild.Id, roleId);
            }
            catch (Exception e)
            {
                rollbackFailed = true;

                logger.LogError(
                    e,
                    "Failed to roll back role {RoleId} in guild {GuildId}.",
                    roleId,
                    guild.Id);
            }
        }

        if (rollbackFailed)
        {
            logger.LogWarning(
                "Rollback for guild {GuildId} was incomplete. Keeping recovery data for a later cleanup attempt.",
                guild.Id);

            return;
        }
        try
        {
            if (!await database.Guild.DeleteGuild(guild.Id)) 
                logger.LogError("Failed to remove guild registration for guild {GuildId} during rollback.", guild.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to roll back guild settings for guild {GuildId}.", guild.Id);
        }
    }

    public async Task RecoverInterruptedSetupAsync(Guild guild)
    {
        if (!await database.Guild.Exists(guild.Id)) return;
        if (await database.Guild.IsSetupComplete(guild.Id)) return;

        var failed = false;

        var messages = await database.GuildSetup.GetMessages(guild.Id);

        foreach (var message in messages)
        {
            try
            {
                try
                {
                    if (guild.Channels.TryGetValue(message.ChannelId, out var channel) && channel is TextChannel textChannel)
                    {
                        await textChannel.DeleteMessageAsync(message.MessageId);
                    }
                }
                catch (RestException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    // The message or channel is already gone.
                }

                await database.GuildSetup.RemoveMessage(guild.Id, message.MessageId);
            }
            catch (Exception e)
            {
                failed = true;

                logger.LogError(
                    e,
                    "Failed to recover setup message {MessageId} in guild {GuildId}.",
                    message.MessageId,
                    guild.Id);
            }
        }

        var roles = await database.GuildSetup.GetRoles(guild.Id);

        foreach (var roleId in roles)
        {
            try
            {
                try
                {
                    await guild.DeleteRoleAsync(roleId);
                }
                catch (RestException e) when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    // The role is already gone.
                }

                await database.GuildSetup.RemoveRole(guild.Id, roleId);
            }
            catch (Exception e)
            {
                failed = true;

                logger.LogError(
                    e,
                    "Failed to recover setup role {RoleId} in guild {GuildId}.",
                    roleId,
                    guild.Id);
            }
        }

        if (failed)
            throw new InvalidRequestException("A previous setup could not be fully cleaned up. Try again later.");

        if (!await database.Guild.DeleteGuild(guild.Id))
            throw new InvalidRequestException(
                "The interrupted setup was cleaned up, but the guild registration could not be removed.");
    }
}