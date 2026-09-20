using EnananV2.Definitions.Models;

namespace EnananV2.Database.Repositories;

public sealed class GuildRepository(SqliteConnector database) : BaseRepository(database)
{
    public async Task<bool> GuildExists(ulong guildId)
    {
        const string sql = """
                           SELECT 1
                           FROM guild_settings
                           WHERE guild_id = @guild_id
                           LIMIT 1
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guild_id", guildId.ToString());
        
        return await cmd.ExecuteScalarAsync() is not null;
    }
    
    public async Task<bool> RegisterGuild(ulong guildId, ulong channelId, RoleMode roleMode, bool enabled = false)
    {
        const string sql = """
                           INSERT INTO guild_settings(guild_id, system_channel_id, guild_role_mode, enabled)
                           VALUES(@guildId, @channelId, @roleMode, @enabled)
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.Parameters.AddWithValue("@channelId", channelId.ToString());
        cmd.Parameters.AddWithValue("@roleMode", roleMode.ToString());
        cmd.Parameters.AddWithValue("@enabled", enabled ? 1 : 0);

        return await cmd.ExecuteNonQueryAsync() == 1;
    }
    
    public async Task<bool> DeleteGuild(ulong guildId)
    {
        const string sql = "DELETE FROM guild_settings WHERE guild_id = @guild_id";
        
        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();
        
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guild_id", guildId.ToString());
        
        return await cmd.ExecuteNonQueryAsync() == 1;
    }
    
    public async Task<bool> IsGuildEnabled(ulong guildId)
    {
        const string sql = """
                           SELECT enabled
                           FROM guild_settings
                           WHERE guild_id = @guildId
                           LIMIT 1
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        var result = await cmd.ExecuteScalarAsync();

        return result is long value && value != 0;
    }
    
    public async Task<bool> SetGuildEnabled(ulong guildId, bool enabled)
    {
        const string sql = """
                           UPDATE guild_settings
                           SET enabled = @enabled
                           WHERE guild_id = @guildId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.Parameters.AddWithValue("@enabled", enabled ? 1 : 0);

        return await cmd.ExecuteNonQueryAsync() == 1;
    }
    
    public async Task<RoleMode> GetRoleMode(ulong guildId)
    {
        const string sql = """
                           SELECT guild_role_mode
                           FROM guild_settings
                           WHERE guild_id = @guild_id
                           LIMIT 1
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guild_id", guildId.ToString());

        var result = await cmd.ExecuteScalarAsync();

        if (result is not string value)
            throw new InvalidOperationException($"Guild {guildId} does not have a stored role mode.");

        return !Enum.TryParse<RoleMode>(value, out var roleMode) 
            ? throw new InvalidOperationException($"Guild {guildId} has an invalid stored role mode '{value}'.") 
            : roleMode;
    }
    
    public async Task<bool> UpdateSystemChannel(ulong guildId, ulong channelId)
    {
        const string sql = """
                           UPDATE guild_settings
                           SET system_channel_id = @channelId
                           WHERE guild_id = @guildId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@channelId", channelId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        return await cmd.ExecuteNonQueryAsync() == 1;
    }
    
    public async Task<ulong?> GetSystemChannel(ulong guildId)
    {
        const string sql = """
                           SELECT system_channel_id
                           FROM guild_settings
                           WHERE guild_id = @guildId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        var result = await cmd.ExecuteScalarAsync();

        return result is string channelId
            ? ulong.Parse(channelId)
            : null;
    }
    
    public async Task<ulong?> GetWelcomeChannel(ulong guildId)
    {
        const string sql = """
                           SELECT welcome_channel_id
                           FROM guild_settings
                           WHERE guild_id = @guildId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        var result = await cmd.ExecuteScalarAsync();

        return result is string channelId
            ? ulong.Parse(channelId)
            : null;
    }
    
    public async Task<bool> UpdateWelcomeChannel(ulong guildId, ulong? channelId)
    {
        const string sql = """
                           UPDATE guild_settings
                           SET welcome_channel_id = @channel_id
                           WHERE guild_id = @guild_id
                           """;

        await using var conn = OpenConnection();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("@guild_id", guildId.ToString());
        cmd.Parameters.AddWithValue("@channel_id", channelId?.ToString() ?? (object)DBNull.Value);

        return await cmd.ExecuteNonQueryAsync() == 1;
    }
    
    public async Task<bool> GetRoleLimitWarned(ulong guildId)
    {
        const string sql = """
                           SELECT role_limit_warned
                           FROM guild_settings
                           WHERE guild_id = @guildId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        var result = await cmd.ExecuteScalarAsync();

        return result is long value && value != 0;
    }

    public async Task<bool> SetRoleLimitWarned(ulong guildId, bool warned)
    {
        const string sql = """
                           UPDATE guild_settings
                           SET role_limit_warned = @warned
                           WHERE guild_id = @guildId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.Parameters.AddWithValue("@warned", warned ? 1 : 0);

        return await cmd.ExecuteNonQueryAsync() == 1;
    }
    
    public async Task<bool> IsSetupComplete(ulong guildId)
    {
        const string sql = """
                           SELECT setup_complete
                           FROM guild_settings
                           WHERE guild_id = @guildId
                           LIMIT 1
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        var result = await cmd.ExecuteScalarAsync();

        return result is long value && value != 0;
    }
    
    public async Task<bool> FinalizeSetup(ulong guildId)
    {
        const string updateSql = """
                                 UPDATE guild_settings
                                 SET setup_complete = 1,
                                     enabled = 1
                                 WHERE guild_id = @guildId
                                 """;

        const string cleanupSql = """
                                  DELETE FROM setup_messages
                                  WHERE guild_id = @guildId;

                                  DELETE FROM setup_roles
                                  WHERE guild_id = @guildId;
                                  """;

        await using var conn = OpenConnection();
        await using var transaction = conn.BeginTransaction();

        try
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = updateSql;
                cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

                if (await cmd.ExecuteNonQueryAsync() != 1)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = cleanupSql;
                cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

                await cmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}