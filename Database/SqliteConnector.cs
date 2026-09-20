using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace EnananV2.Database;

public sealed class SqliteConnector
{
    private readonly string _connectionString;

    public SqliteConnector(IConfiguration config)
    {
        var dbPath = config["Database:Path"]
                     ?? throw new InvalidOperationException("Database not configured.");

        if (!Path.IsPathRooted(dbPath))
            dbPath = Path.Combine(AppContext.BaseDirectory, dbPath);

        var dbDirectory = Path.GetDirectoryName(dbPath);

        if (!string.IsNullOrEmpty(dbDirectory))
            Directory.CreateDirectory(dbDirectory);

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true,
            DefaultTimeout = 30,
            ForeignKeys = true,
        }.ToString();
    }

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);

        connection.Open();
        ConfigureConnection(connection);

        return connection;
    }

    private static void ConfigureConnection(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = """
                              PRAGMA journal_mode = WAL;
                              PRAGMA synchronous = NORMAL;
                              PRAGMA temp_store = MEMORY;
                              PRAGMA foreign_keys = ON;
                              PRAGMA cache_size = -200000;
                              """;

        command.ExecuteNonQuery();
    }
}