using EnananV2.Database;
using Microsoft.Data.Sqlite;

namespace EnananV2.Definitions.Models;

public abstract class BaseRepository(SqliteConnector database)
{
    protected SqliteConnection OpenConnection() => database.Open();
}