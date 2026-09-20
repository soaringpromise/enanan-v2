using Microsoft.Extensions.Logging;

namespace EnananV2.Services;

public sealed class GuildNotificationService(
    DatabaseService database,
    ResponseService responses,
    ILogger<GuildNotificationService> logger)
{
    public Task NotifyManualInterventionRequired(ulong guildId, string message)
        => SendSystemNotification(guildId, message);

    public async Task SendSystemNotification(ulong guildId, string message)
    {
        try
        {
            var systemChannelId = await database.Guild.GetSystemChannelId(guildId);
            if (systemChannelId is null) return;

            await responses.SendSystemMessage(systemChannelId.Value, message);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to send system notification to guild {GuildId}.",
                guildId);
        }
    }
}