using Dapper;
using Microsoft.Data.Sqlite;

namespace AttendanceRecord.Persistence.Storage;

public sealed class WorkRecordMigrationSqliteStore(string dbPath)
{
    private const string UpsertWorkSql =
        """
        INSERT INTO work_records (id, started_at, ended_at, work_date)
        VALUES (@Id, @StartedAt, @EndedAt, @WorkDate)
        ON CONFLICT(id) DO UPDATE SET
           started_at = excluded.started_at,
           ended_at = excluded.ended_at,
           work_date = excluded.work_date;
        """;

    private const string DeleteRestByWorkIdSql = "DELETE FROM rest_records WHERE work_record_id = @WorkRecordId;";

    private const string InsertRestSql =
        """
        INSERT INTO rest_records (id, work_record_id, started_at, ended_at, variant)
        VALUES (@Id, @WorkRecordId, @StartedAt, @EndedAt, @Variant);
        """;

    private const string HasAnyDataSql =
        """
        SELECT CASE
           WHEN EXISTS(SELECT 1 FROM work_records LIMIT 1)
           THEN 1 ELSE 0 END;
        """;

    private readonly string _connectionString = $"Data Source={dbPath}";

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await SqliteSchemaManager.EnsureAsync(connection, cancellationToken);
        return connection;
    }

    public async Task<bool> HasAnyDataAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<int>(HasAnyDataSql) == 1;
    }

    public async Task SaveManyAsync(
        IReadOnlyList<WorkRecordSqlRow> workRecords,
        IReadOnlyList<RestRecordSqlRow> restRecords,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var restRowsByWorkId = restRecords
            .GroupBy(row => row.WorkRecordId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<RestRecordSqlRow>)group.ToList());

        foreach (var workRecord in workRecords)
        {
            await connection.ExecuteAsync(UpsertWorkSql, workRecord, transaction);
            await connection.ExecuteAsync(
                DeleteRestByWorkIdSql,
                new { WorkRecordId = workRecord.Id },
                transaction);

            if (restRowsByWorkId.TryGetValue(workRecord.Id, out var rows) && rows.Count > 0)
            {
                await connection.ExecuteAsync(InsertRestSql, rows, transaction);
            }
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
