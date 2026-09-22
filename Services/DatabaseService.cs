using EnananV2.Database.Repositories;
using EnananV2.Definitions.Models;
using NodaTime;

namespace EnananV2.Services;

public sealed class DatabaseService(
    GuildRepository guildRepository,
    GuildMemberRepository guildMemberRepository,
    TierProfileRepository tierProfileRepository,
    GuildSetupRepository guildSetupRepository)
{
    public Guilds Guild { get; } = new(guildRepository);
    public GuildMembers GuildMember { get; } = new(guildMemberRepository);
    public TierProfiles TierProfile { get; } = new(tierProfileRepository);
    public GuildSetupOperations GuildSetup { get; } = new(guildSetupRepository);

    public sealed class GuildSetupOperations(GuildSetupRepository repository)
    {
        public Task AddRole(ulong guildId, ulong roleId)
            => repository.AddRole(guildId, roleId);

        public Task AddMessage(ulong guildId, ulong channelId, ulong messageId)
            => repository.AddMessage(guildId, channelId, messageId);

        public Task<IReadOnlyList<ulong>> GetRoles(ulong guildId)
            => repository.GetRoles(guildId);

        public Task<IReadOnlyList<SetupMessageReference>> GetMessages(ulong guildId)
            => repository.GetMessages(guildId);

        public Task RemoveRole(ulong guildId, ulong roleId)
            => repository.RemoveRole(guildId, roleId);

        public Task RemoveMessage(ulong guildId, ulong messageId)
            => repository.RemoveMessage(guildId, messageId);
    }
    
    public sealed class Guilds(GuildRepository repository)
    {
        public Task<bool> Exists(ulong guildId)
            => repository.GuildExists(guildId);
        
        public Task<bool> Register(ulong guildId, ulong systemChannelId, RoleMode roleMode, bool enabled = false)
            => repository.RegisterGuild(guildId, systemChannelId, roleMode, enabled);
        
        public Task<bool> DeleteGuild(ulong guildId)
            => repository.DeleteGuild(guildId);
        
        public Task<bool> IsEnabled(ulong guildId)
            => repository.IsGuildEnabled(guildId);
        
        public Task<bool> DisableGuild(ulong guildId)
            => repository.SetGuildEnabled(guildId, false);
        
        public Task<RoleMode> GetRoleMode(ulong guildId)
            => repository.GetRoleMode(guildId);
        
        public Task<bool> UpdateSystemChannel(ulong guildId, ulong channelId)
            => repository.UpdateSystemChannel(guildId, channelId);

        public Task<ulong?> GetSystemChannelId(ulong guildId)
            => repository.GetSystemChannel(guildId);
        
        public Task<ulong?> GetWelcomeChannelId(ulong guildId)
            => repository.GetWelcomeChannel(guildId);
        
        public Task<bool> AssignWelcomeChannel(ulong guildId, ulong channelId)
            => repository.UpdateWelcomeChannel(guildId, channelId);
        
        public Task<bool> RemoveWelcomeChannel(ulong guildId)
            => repository.UpdateWelcomeChannel(guildId, null);
        
        public Task<bool> SetStaticRoleAnchorIfUnset(ulong guildId, ulong roleId)
            => repository.SetStaticRoleAnchorIfUnset(guildId, roleId);
        
        public Task<ulong?> GetStaticRoleAnchorId(ulong guildId)
            => repository.GetStaticRoleAnchorId(guildId);

        public Task<bool> ClearStaticRoleAnchor(ulong guildId, ulong roleId)
            => repository.ClearStaticRoleAnchor(guildId, roleId);
        
        public Task<bool> WasRoleLimitWarned(ulong guildId)
            => repository.GetRoleLimitWarned(guildId);

        public Task<bool> MarkRoleLimitWarned(ulong guildId)
            => repository.SetRoleLimitWarned(guildId, true);

        public Task<bool> ClearRoleLimitWarning(ulong guildId)
            => repository.SetRoleLimitWarned(guildId, false);
        
        public Task<bool> IsSetupComplete(ulong guildId)
            => repository.IsSetupComplete(guildId);
        
        public Task<bool> FinalizeSetup(ulong guildId)
            => repository.FinalizeSetup(guildId);
    }

    public sealed class GuildMembers(GuildMemberRepository repository)
    {
        public Task<int> RegisterAllUsers(ulong guildId, IEnumerable<ulong> userIds)
            => repository.RegisterUsersInGuild(guildId, userIds);
        
        public Task<bool> RegisterUser(ulong guildId, ulong userId)
            => repository.RegisterUserInGuild(guildId, userId);

        public Task<bool> DeleteUser(ulong guildId, ulong userId)
            => repository.DeleteUserFromGuild(guildId, userId);
        
        public Task<bool> AssignUserCustomRole(ulong guildId, ulong userId, ulong roleId)
            => repository.UpdateUserCustomRoleId(guildId, userId, roleId);

        public Task<bool> RemoveUserCustomRole(ulong guildId, ulong userId)
            => repository.UpdateUserCustomRoleId(guildId, userId, null);

        public Task<ulong?> GetCustomRoleId(ulong guildId, ulong userId)
            => repository.GetUserCustomRoleIdInGuild(guildId, userId);
        
        public Task ClearCustomRoleById(ulong guildId, ulong roleId)
            => repository.ClearCustomRoleById(guildId, roleId);
    }

    public sealed class TierProfiles(TierProfileRepository repository)
    {
        public Task<bool> Exists(ulong userId)
            => repository.TierProfileExists(userId);
        
        public Task<bool> Create(TierProfile tierProfile) 
            => repository.CreateTierProfile(tierProfile);
        
        public Task<TierProfile?> GetTierProfile(ulong userId)
            => repository.GetTierProfile(userId);
        
        public Task<bool> Delete(ulong userId)
            => repository.DeleteTierProfile(userId);
        
        public Task<bool> UpdateTierTarget(ulong userId, TierBracket tier)
            => repository.UpdateTierTarget(userId, tier);

        public Task<bool> UpdateServer(ulong userId, GameServer server)
            => repository.UpdateServer(userId, server);

        public Task<bool> UpdateTimezone(ulong userId, DateTimeZone timezone)
            => repository.UpdateTimezone(userId, timezone);

        public Task<bool> UpdateTeam(ulong userId, TeamSlot team, TeamInfo info)
            => repository.UpdateTeam(userId, team, info);

        public Task<bool> UpdatePlaystyle(ulong userId, Playstyle playstyle)
            => repository.UpdatePlaystyle(userId, playstyle);

        public Task<bool> UpdateFavoriteCard(ulong userId, int cardId)
            => repository.UpdateFavoriteCard(userId, cardId);
    }
}