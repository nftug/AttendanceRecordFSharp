using Dapper;
using Microsoft.Data.Sqlite;

namespace AttendanceRecord.Persistence.Storage;

public static class SqliteSchemaManager
{
    private static readonly string[] SchemaSql =
    [
        "PRAGMA foreign_keys = ON;",
        """
        CREATE TABLE IF NOT EXISTS work_records (
           id TEXT PRIMARY KEY,
           started_at TEXT NOT NULL,
           ended_at TEXT NULL,
           work_date TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS rest_records (
           id TEXT PRIMARY KEY,
           work_record_id TEXT NOT NULL,
           started_at TEXT NOT NULL,
           ended_at TEXT NULL,
           variant INTEGER NOT NULL,
           FOREIGN KEY(work_record_id) REFERENCES work_records(id) ON DELETE CASCADE
        );
        """,
        "CREATE INDEX IF NOT EXISTS idx_work_records_work_date ON work_records(work_date);",
        "CREATE INDEX IF NOT EXISTS idx_work_records_started_at ON work_records(started_at);",
        "CREATE INDEX IF NOT EXISTS idx_rest_records_work_record_id ON rest_records(work_record_id);"
    ];

    public static async Task EnsureAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        foreach (var sql in SchemaSql)
        {
            await connection.ExecuteAsync(sql);
        }
    }
}
