using EnananV2.Definitions.Models;
using Microsoft.Data.Sqlite;
using NodaTime;

namespace EnananV2.Database.Repositories;

public sealed class TierProfileRepository(SqliteConnector database) : BaseRepository(database)
{
    public async Task<bool> TierProfileExists(ulong userId)
    {
        const string sql = "SELECT 1 FROM tier_profiles WHERE user_id = @userId LIMIT 1";

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("@userId", userId.ToString());

        return await cmd.ExecuteScalarAsync() != null;
    }

    public async Task<bool> CreateTierProfile(TierProfile profile)
    {
        const string sql = """
                           INSERT INTO tier_profiles
                               (user_id,
                                usual_tier, game_server, playstyle,
                                primary_talent, primary_isv,
                                heal_talent, heal_isv,
                                encore_talent, encore_isv,
                                favorite_card_id,
                                highest_tier, event_id,
                                timezone, game_id)
                           VALUES
                               (@userId,
                                @usualTier, @gameServer, @playstyle,
                                @primaryTalent, @primaryIsv,
                                @healTalent, @healIsv,
                                @encoreTalent, @encoreIsv,
                                @favoriteCard,
                                @highestTier, @eventId,
                                @timezone, @gameId)
                           ON CONFLICT (user_id) DO NOTHING
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;

        cmd.Parameters.AddWithValue("@userId", profile.UserId.ToString());

        cmd.Parameters.AddWithValue("@usualTier", (int)profile.UsualTier);
        cmd.Parameters.AddWithValue("@gameServer", (int)profile.Server);
        cmd.Parameters.AddWithValue("@playstyle", (int)profile.Playstyle);

        cmd.Parameters.AddWithValue("@primaryTalent", profile.Primary.Talent);
        cmd.Parameters.AddWithValue("@primaryIsv", profile.Primary.Isv);

        cmd.Parameters.AddWithValue("@healTalent", (object?)profile.Heal?.Talent ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@healIsv", (object?)profile.Heal?.Isv ?? DBNull.Value);

        cmd.Parameters.AddWithValue("@encoreTalent", (object?)profile.Encore?.Talent ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@encoreIsv", (object?)profile.Encore?.Isv ?? DBNull.Value);

        cmd.Parameters.AddWithValue("@favoriteCard", profile.FavoriteCardId);

        cmd.Parameters.AddWithValue("@highestTier",
            profile.HighestTier.HasValue ? (int)profile.HighestTier.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@eventId", (object?)profile.EventId ?? DBNull.Value);

        cmd.Parameters.AddWithValue("@timezone", profile.TimeZone.Id);
        cmd.Parameters.AddWithValue("@gameId", profile.GameId);

        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    public async Task<TierProfile?> GetTierProfile(ulong userId)
    {
        const string sql = """
                           SELECT usual_tier, game_server, playstyle,
                                  primary_talent, primary_isv,
                                  heal_talent, heal_isv,
                                  encore_talent, encore_isv,
                                  favorite_card_id,
                                  highest_tier, event_id,
                                  timezone, game_id
                           FROM tier_profiles
                           WHERE user_id = @userId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@userId", userId.ToString());

        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        var timeZoneId = reader.GetString(reader.GetOrdinal("timezone"));

        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId)
                       ?? throw new InvalidOperationException(
                           $"Invalid timezone '{timeZoneId}' stored for user {userId}.");

        var primary = new TeamInfo(
            reader.GetInt32(reader.GetOrdinal("primary_talent")),
            reader.GetString(reader.GetOrdinal("primary_isv")));

        var heal = ReadOptionalTeam(reader, "heal_talent", "heal_isv");

        var encore = ReadOptionalTeam(reader, "encore_talent", "encore_isv");

        var highestTierOrdinal = reader.GetOrdinal("highest_tier");
        var eventIdOrdinal = reader.GetOrdinal("event_id");

        return new TierProfile(
            userId,
            ReadEnum<TierBracket>(reader, "usual_tier"),
            ReadEnum<GameServer>(reader, "game_server"),
            ReadEnum<Playstyle>(reader, "playstyle"),
            primary,
            heal,
            encore,
            reader.GetInt32(reader.GetOrdinal("favorite_card_id")),
            timeZone,
            reader.GetString(reader.GetOrdinal("game_id")),
            reader.IsDBNull(highestTierOrdinal)
                ? null
                : (TierBracket)reader.GetInt32(highestTierOrdinal),
            reader.IsDBNull(eventIdOrdinal)
                ? null
                : reader.GetInt32(eventIdOrdinal));
    }

    public async Task<bool> DeleteTierProfile(ulong userId)
    {
        const string sql = """
                           DELETE FROM tier_profiles
                           WHERE user_id = @userId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@userId", userId.ToString());

        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> UpdateTierTarget(ulong userId, TierBracket tier)
    {
        const string sql = """
                           UPDATE tier_profiles
                           SET usual_tier = @tier
                           WHERE user_id = @userId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@tier", (int)tier);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());

        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> UpdateServer(ulong userId, GameServer server)
    {
        const string sql = """
                           UPDATE tier_profiles
                           SET game_server = @server
                           WHERE user_id = @userId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@server", (int)server);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());

        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> UpdateTimezone(ulong userId, DateTimeZone timezone)
    {
        const string sql = """
                           UPDATE tier_profiles
                           SET timezone = @timezone
                           WHERE user_id = @userId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@timezone", timezone.Id);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());

        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> UpdateTeam(ulong userId, TeamSlot team, TeamInfo info)
    {
        var (talentColumn, isvColumn) = team switch
        {
            TeamSlot.Primary => ("primary_talent", "primary_isv"),
            TeamSlot.Heal => ("heal_talent", "heal_isv"),
            TeamSlot.Encore => ("encore_talent", "encore_isv"),
            _ => throw new ArgumentOutOfRangeException(nameof(team))
        };

        var sql = $"""
                   UPDATE tier_profiles
                   SET {talentColumn} = @talent,
                       {isvColumn} = @isv
                   WHERE user_id = @userId
                   """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@talent", info.Talent);
        cmd.Parameters.AddWithValue("@isv", info.Isv);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());

        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> UpdatePlaystyle(ulong userId, Playstyle playstyle)
    {
        const string sql = """
                           UPDATE tier_profiles
                           SET playstyle = @playstyle
                           WHERE user_id = @userId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@playstyle", (int)playstyle);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());

        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> UpdateFavoriteCard(ulong userId, int cardId)
    {
        const string sql = """
                           UPDATE tier_profiles
                           SET favorite_card_id = @cardId
                           WHERE user_id = @userId
                           """;

        await using var conn = OpenConnection();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@cardId", cardId);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());

        return await cmd.ExecuteNonQueryAsync() == 1;
    }
    
    private static TeamInfo? ReadOptionalTeam(SqliteDataReader reader, string talentColumn, string isvColumn)
    {
        var talentOrdinal = reader.GetOrdinal(talentColumn);
        var isvOrdinal = reader.GetOrdinal(isvColumn);

        if (reader.IsDBNull(talentOrdinal) && reader.IsDBNull(isvOrdinal)) return null;

        if (reader.IsDBNull(talentOrdinal) || reader.IsDBNull(isvOrdinal))
            throw new InvalidOperationException(
                $"Invalid team data: '{talentColumn}' and '{isvColumn}' must both be null or both contain values.");

        return new TeamInfo(reader.GetInt32(talentOrdinal), reader.GetString(isvOrdinal));
    }
    
    private static TEnum ReadEnum<TEnum>(SqliteDataReader reader, string column) where TEnum : struct, Enum
    {
        var value = reader.GetInt32(reader.GetOrdinal(column));

        if (!Enum.IsDefined(typeof(TEnum), value))
            throw new InvalidOperationException($"Invalid {typeof(TEnum).Name} value '{value}' stored in column '{column}'.");
        return (TEnum)Enum.ToObject(typeof(TEnum), value);
    }
}