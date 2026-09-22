using EnananV2.Services;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace EnananV2.Discord.Events;

public sealed class OnRoleDeleteEvent(
    DatabaseService database,
    ILogger<OnRoleDeleteEvent> logger) : IRoleDeleteGatewayHandler
{
    public async ValueTask HandleAsync(RoleDeleteEventArgs arg)
    {
        if (!await database.Guild.Exists(arg.GuildId)) return;
        try
        {
            await database.GuildMember.ClearCustomRoleById(arg.GuildId, arg.RoleId);
            await database.Guild.ClearStaticRoleAnchor(arg.GuildId, arg.RoleId);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to clean up deleted role {RoleId} in guild {GuildId}.",
                arg.RoleId,
                arg.GuildId);
        }
    }
}