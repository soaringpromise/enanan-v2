using Microsoft.Data.Sqlite;

namespace EnananV2.Database.Configuration;

public sealed class DatabaseMigrator(SqliteConnector connector)
{
    public const int CurrentVersion = 1;

    public void Migrate()
    {
        using var connection = connector.Open();
        using var transaction = connection.BeginTransaction();

        var version = GetVersion(connection, transaction);

        while (version < CurrentVersion)
        {
            version++;

            ApplyMigration(connection, transaction, version);
            SetVersion(connection, transaction, version);
        }

        transaction.Commit();
    }

    private static int GetVersion(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText =
            """
            SELECT version
            FROM schema_version
            WHERE id = 1
            """;

        var result = command.ExecuteScalar();

        return result is null
            ? throw new InvalidOperationException("Database schema version has not been initialized.")
            : Convert.ToInt32(result);
    }

    private static void SetVersion(
        SqliteConnection connection,
        SqliteTransaction transaction,
        int version)
    {
        using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText =
            """
            UPDATE schema_version
            SET version = @version
            WHERE id = 1
            """;

        command.Parameters.AddWithValue("@version", version);
        command.ExecuteNonQuery();
    }

    private static void ApplyMigration(SqliteConnection connection, SqliteTransaction transaction, int version)
    {
        throw version switch
        {
            _ => new InvalidOperationException($"No migration exists for database version {version}.")
        };
    }
}