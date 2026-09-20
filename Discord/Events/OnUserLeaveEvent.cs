using System.Net;
using EnananV2.Services;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace EnananV2.Discord.Events;

public sealed class OnUserLeaveEvent(
    DatabaseService database,
    GuildNotificationService notifications,
    RestClient client,
    ILogger<OnUserLeaveEvent> logger)
    : IGuildUserRemoveGatewayHandler
{
    public async ValueTask HandleAsync(GuildUserRemoveEventArgs arg)
    {
        if (arg.User.IsBot || !await database.Guild.Exists(arg.GuildId)) return;
        try
        {
            var roleId = await database.GuildMember.GetCustomRoleId(arg.GuildId, arg.User.Id);
            if (roleId is not null)
            {
                try
                {
                    await client.DeleteGuildRoleAsync(arg.GuildId, roleId.Value);
                }
                catch (RestException e)when (e.StatusCode == HttpStatusCode.NotFound)
                {
                    // Already deleted manually. That's fine.
                }
            }

            var removed = await database.GuildMember.DeleteUser(arg.GuildId, arg.User.Id);
            if (!removed) throw new InvalidOperationException($"Could not unregister user {arg.User.Id}.");
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to clean up user {UserId} from guild {GuildId}.",
                arg.User.Id,
                arg.GuildId);

            await notifications.NotifyManualInterventionRequired(
                arg.GuildId,
                $"Failed to fully clean up <@{arg.User.Id}> after they left the server.");
        }
    }
}