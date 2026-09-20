using EnananV2.Services;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EnananV2.Discord.Events;

public sealed class OnGuildLeaveEvent(
    DatabaseService database,
    ILogger<OnGuildLeaveEvent> logger) : IGuildDeleteGatewayHandler
{
    public async ValueTask HandleAsync(GuildDeleteEventArgs arg)
    {
        // Discord may temporarily mark a guild as unavailable.
        // Do not delete its data unless the bot has actually left the guild.
        if (arg.IsUnavailable) return;

        try
        {
            var deleted = await database.Guild.DeleteGuild(arg.GuildId);

            if (!deleted) 
                logger.LogWarning("Guild {GuildId} was not found in the database.", arg.GuildId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to remove guild {GuildId} from the database.", arg.GuildId);
        }
    }
}