using EnananV2.Services;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Hosting.Gateway;

namespace EnananV2.Discord.Events;

public sealed class OnUserJoinEvent(
    DatabaseService database,
    ResponseService responses,
    GuildNotificationService notifications,
    ILogger<OnUserJoinEvent> logger) : IGuildUserAddGatewayHandler
{
    public async ValueTask HandleAsync(GuildUser user)
    {
        if (user.IsBot || !await database.Guild.Exists(user.GuildId)) return;

        try
        {
            await database.GuildMember.RegisterUser(user.GuildId, user.Id);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to register user {UserId} in guild {GuildId}.",
                user.Id,
                user.GuildId);

            await notifications.NotifyManualInterventionRequired(
                user.GuildId,
                $"Failed to automatically register <@{user.Id}>. Please register the user manually.");

            return;
        }

        try
        {
            if (!await database.Guild.IsSetupComplete(user.GuildId)) return;
            if (!await database.Guild.IsEnabled(user.GuildId)) return;

            var welcomeChannelId = await database.Guild.GetWelcomeChannelId(user.GuildId);
            if (welcomeChannelId is not null) await responses.SendWelcomeMessage(welcomeChannelId.Value, user.Id);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to send welcome message for user {UserId} in guild {GuildId}.",
                user.Id,
                user.GuildId);

            await notifications.NotifyManualInterventionRequired(
                user.GuildId,
                $"Failed to send the welcome message for <@{user.Id}>.");
        }
    }
}