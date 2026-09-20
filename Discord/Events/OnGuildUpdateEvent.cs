using EnananV2.Definitions.Models;
using EnananV2.Services;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EnananV2.Discord.Events;

public sealed class OnGuildUpdateEvent(
    DatabaseService database,
    GuildNotificationService notifications,
    ILogger<OnGuildUpdateEvent> logger) : IGuildUpdateGatewayHandler
{
    private const int DiscordRoleLimit = 250;
    private const int MaxStaticRoles = 40;
    private const int WarningThreshold = 200;
    private const int DisableThreshold = 240;

    public async ValueTask HandleAsync(Guild guild)
    {
        if (!await database.Guild.Exists(guild.Id)) return;

        try
        {
            if (!await database.Guild.IsEnabled(guild.Id)) return;

            var roleMode = await database.Guild.GetRoleMode(guild.Id);
            var projectedRoles = CalculateProjectedRoles(guild, roleMode);

            var warned = await database.Guild.WasRoleLimitWarned(guild.Id);

            switch (projectedRoles)
            {
                case < WarningThreshold:
                {
                    if (warned) await database.Guild.ClearRoleLimitWarning(guild.Id);
                    return;
                }
                case >= DisableThreshold:
                    await database.Guild.DisableGuild(guild.Id);

                    await notifications.SendSystemNotification(
                        guild.Id,
                        $"**WARNING:** Enanan has been disabled because this server is too close to Discord's " +
                        $"role limit. Some Enanan features may require creating additional roles. " +
                        $"({guild.Roles.Count}/{DiscordRoleLimit} roles currently in use.)");

                    return;
            }

            if (warned) return;

            await database.Guild.MarkRoleLimitWarned(guild.Id);

            await notifications.SendSystemNotification(
                guild.Id,
                $"**WARNING:** This server is approaching Discord's role limit. " +
                $"Some Enanan features may stop working correctly if too many roles are created. " +
                $"({guild.Roles.Count}/{DiscordRoleLimit} roles currently in use.)");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to handle guild update for guild {GuildId}.",
                guild.Id);
        }
    }

    private static int CalculateProjectedRoles(Guild guild, RoleMode roleMode)
    {
        var projectedRoles = guild.Roles.Count;

        if (roleMode is RoleMode.ProjectSekai or RoleMode.Both)
            projectedRoles += MaxStaticRoles;

        if (roleMode is RoleMode.Custom or RoleMode.Both)
            projectedRoles += Math.Min(guild.Users.Count / 10, 50);

        return projectedRoles;
    }
}