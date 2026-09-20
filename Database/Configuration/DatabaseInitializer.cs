using Microsoft.Data.Sqlite;

namespace EnananV2.Database.Configuration;

public sealed class DatabaseInitializer(SqliteConnector connector)
{
    private const int InitialSchemaVersion = DatabaseMigrator.CurrentVersion;

    private readonly string _schemaPath = 
        Path.Combine(AppContext.BaseDirectory, "Database", "Configuration", "schema.sql");

    public void Initialize()
    {
        if (!File.Exists(_schemaPath))
            throw new FileNotFoundException("Database schema file could not be found.", _schemaPath);

        using var connection = connector.Open();
        using var transaction = connection.BeginTransaction();

        ApplySchema(connection, transaction);
        InitializeVersion(connection, transaction);

        transaction.Commit();
    }

    private void ApplySchema(SqliteConnection connection, SqliteTransaction transaction)
    {
        var schema = File.ReadAllText(_schemaPath);

        using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText = schema;

        command.ExecuteNonQuery();
    }

    private static void InitializeVersion(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO schema_version (id, version)
            VALUES (1, @version)
            ON CONFLICT(id) DO NOTHING;
            """;

        command.Parameters.AddWithValue("@version", InitialSchemaVersion);

        command.ExecuteNonQuery();
    }
}