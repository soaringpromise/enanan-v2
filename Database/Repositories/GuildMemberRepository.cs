using EnananV2.Definitions.Models;

namespace EnananV2.Database.Repositories;

public sealed class GuildMemberRepository(SqliteConnector database) : BaseRepository(database)
{
    public async Task<int> RegisterUsersInGuild(ulong guildId, IEnumerable<ulong> userIds)
    {
        const string sql = """
                           INSERT INTO user_roles (guild_id, user_id, role_id)
                           VALUES (@guildId, @userId, NULL)
                           ON CONFLICT (guild_id, user_id) DO NOTHING
                           """;

        await using var conn = OpenConnection();
        await using var transaction = conn.BeginTransaction();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = transaction;

        var guildIdParam = cmd.CreateParameter();
        guildIdParam.ParameterName = "@guildId";
        guildIdParam.Value = guildId.ToString();
        cmd.Parameters.Add(guildIdParam);

        var userIdParam = cmd.CreateParameter();
        userIdParam.ParameterName = "@userId";
        cmd.Parameters.Add(userIdParam);

        var totalInserted = 0;

        foreach (var userId in userIds)
        {
            userIdParam.Value = userId.ToString();
            totalInserted += await cmd.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return totalInserted;
    }
    
    public async Task<bool> RegisterUserInGuild(ulong guildId, ulong userId)
    {
        const string sql = """
                           INSERT INTO user_roles (user_id, guild_id, role_id)
                           VALUES (@userId, @guildId, NULL)
                           ON CONFLICT (user_id, guild_id) DO NOTHING
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@userId", userId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> DeleteUserFromGuild(ulong guildId, ulong userId)
    {
        const string sql = """
                           DELETE FROM user_roles
                           WHERE user_id = @userId AND guild_id = @guildId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@userId", userId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        
        return await cmd.ExecuteNonQueryAsync() == 1;
    }
    
    public async Task<bool> UpdateUserCustomRoleId(ulong guildId, ulong userId, ulong? roleId)
    {
        const string sql = """
                           UPDATE user_roles
                           SET role_id = @role_id
                           WHERE user_id = @user_id
                           AND guild_id = @guild_id
                           """;

        await using var conn = OpenConnection();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("@guild_id", guildId.ToString());
        cmd.Parameters.AddWithValue("@user_id", userId.ToString());
        cmd.Parameters.AddWithValue("@role_id", roleId?.ToString() ?? (object)DBNull.Value);

        return await cmd.ExecuteNonQueryAsync() == 1;
    }
    
    public async Task<ulong?> GetUserCustomRoleIdInGuild(ulong guildId, ulong userId)
    {
        const string sql = """
                           SELECT role_id
                           FROM user_roles
                           WHERE user_id = @userId
                             AND guild_id = @guildId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@userId", userId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        return reader.IsDBNull(0)
            ? null
            : ulong.Parse(reader.GetString(0));
    }
    
    public async Task ClearCustomRoleById(ulong guildId, ulong roleId)
    {
        const string sql = """
                           UPDATE user_roles
                           SET role_id = NULL
                           WHERE guild_id = @guildId
                             AND role_id = @roleId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.Parameters.AddWithValue("@roleId", roleId.ToString());

        await cmd.ExecuteNonQueryAsync();
    }
}