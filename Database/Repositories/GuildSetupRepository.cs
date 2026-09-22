using EnananV2.Definitions.Models;

namespace EnananV2.Database.Repositories;

public sealed class GuildSetupRepository(SqliteConnector connector)
{
    public async Task AddRole(ulong guildId, ulong roleId)
    {
        const string sql = """
                           INSERT INTO setup_roles (guild_id, role_id)
                           VALUES (@guildId, @roleId);
                           """;
        
        await using var connection = connector.Open();
        await using var command = connection.CreateCommand();

        command.CommandText = sql;
        command.Parameters.AddWithValue("@guildId", guildId.ToString());
        command.Parameters.AddWithValue("@roleId", roleId.ToString());

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddMessage(ulong guildId, ulong channelId, ulong messageId)
    {
        const string sql = """
                           INSERT INTO setup_messages (guild_id, channel_id, message_id)
                           VALUES (@guildId, @channelId, @messageId);
                           """;
        
        await using var connection = connector.Open();
        await using var command = connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.AddWithValue("@guildId", guildId.ToString());
        command.Parameters.AddWithValue("@channelId", channelId.ToString());
        command.Parameters.AddWithValue("@messageId", messageId.ToString());

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<ulong>> GetRoles(ulong guildId)
    {
        const string sql = """
                           SELECT role_id
                           FROM setup_roles
                           WHERE guild_id = @guildId;
                           """;
        
        await using var connection = connector.Open();
        await using var command = connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.AddWithValue("@guildId", guildId.ToString());

        var roles = new List<ulong>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) roles.Add(ulong.Parse(reader.GetString(0)));

        return roles;
    }

    public async Task<IReadOnlyList<SetupMessageReference>> GetMessages(ulong guildId)
    {
        const string sql = """
                           SELECT channel_id, message_id
                           FROM setup_messages
                           WHERE guild_id = @guildId;
                           """;
        
        await using var connection = connector.Open();
        await using var command = connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.AddWithValue("@guildId", guildId.ToString());

        var messages = new List<SetupMessageReference>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            messages.Add(new SetupMessageReference(
                ulong.Parse(reader.GetString(0)),
                ulong.Parse(reader.GetString(1))));
        }

        return messages;
    }

    public async Task RemoveRole(ulong guildId, ulong roleId)
    {
        const string sql = """
                           DELETE FROM setup_roles
                           WHERE guild_id = @guildId
                             AND role_id = @roleId;
                           """;
        
        await using var connection = connector.Open();
        await using var command = connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.AddWithValue("@guildId", guildId.ToString());
        command.Parameters.AddWithValue("@roleId", roleId.ToString());

        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveMessage(ulong guildId, ulong messageId)
    {
        const string sql = """
                           DELETE FROM setup_messages
                           WHERE guild_id = @guildId
                             AND message_id = @messageId;
                           """;
        
        await using var connection = connector.Open();
        await using var command = connection.CreateCommand();

        command.CommandText = sql;
        
        command.Parameters.AddWithValue("@guildId", guildId.ToString());
        command.Parameters.AddWithValue("@messageId", messageId.ToString());

        await command.ExecuteNonQueryAsync();
    }
}