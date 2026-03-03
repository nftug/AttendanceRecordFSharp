using Dapper;
using Microsoft.Data.Sqlite;

namespace AttendanceRecord.Persistence.Storage;

public sealed class WorkRecordSqliteStore(string dbPath)
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

    private const string DeleteWorkSql = "DELETE FROM work_records WHERE id = @Id;";
    private const string DeleteRestByWorkIdSql = "DELETE FROM rest_records WHERE work_record_id = @WorkRecordId;";

    private const string InsertRestSql =
        """
        INSERT INTO rest_records (id, work_record_id, started_at, ended_at, variant)
        VALUES (@Id, @WorkRecordId, @StartedAt, @EndedAt, @Variant);
        """;

    private const string SelectWorkByDateSql =
        """
        SELECT
           id AS Id,
           started_at AS StartedAt,
           ended_at AS EndedAt,
           work_date AS WorkDate
        FROM work_records
        WHERE work_date = @WorkDate
        ORDER BY started_at ASC
        LIMIT 1;
        """;

    private const string SelectWorkByIdSql =
        """
        SELECT
           id AS Id,
           started_at AS StartedAt,
           ended_at AS EndedAt,
           work_date AS WorkDate
        FROM work_records
        WHERE id = @Id
        LIMIT 1;
        """;

    private const string SelectWorkByMonthSql =
        """
        SELECT
           id AS Id,
           started_at AS StartedAt,
           ended_at AS EndedAt,
           work_date AS WorkDate
        FROM work_records
        WHERE work_date >= @StartDate
          AND work_date < @EndDate
        ORDER BY started_at ASC;
        """;

    private const string SelectRestsByWorkIdSql =
        """
        SELECT
           id AS Id,
           work_record_id AS WorkRecordId,
           started_at AS StartedAt,
           ended_at AS EndedAt,
           variant AS Variant
        FROM rest_records
        WHERE work_record_id = @WorkRecordId
        ORDER BY started_at ASC;
        """;

    private const string SelectRestsByMonthSql =
        """
        SELECT
           rest.id AS Id,
           rest.work_record_id AS WorkRecordId,
           rest.started_at AS StartedAt,
           rest.ended_at AS EndedAt,
           rest.variant AS Variant
        FROM rest_records AS rest
        INNER JOIN work_records AS work
           ON work.id = rest.work_record_id
        WHERE work.work_date >= @StartDate
          AND work.work_date < @EndDate
        ORDER BY rest.work_record_id ASC, rest.started_at ASC;
        """;

    private readonly string _connectionString = $"Data Source={dbPath}";
    private readonly SemaphoreSlim _schemaEnsureLock = new(1, 1);
    private bool _isSchemaEnsured;

    private async Task EnsureSchemaOnceAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        if (_isSchemaEnsured) return;

        await _schemaEnsureLock.WaitAsync(cancellationToken);
        try
        {
            if (_isSchemaEnsured) return;

            await SqliteSchemaManager.EnsureAsync(connection, cancellationToken);
            _isSchemaEnsured = true;
        }
        finally
        {
            _schemaEnsureLock.Release();
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
        await EnsureSchemaOnceAsync(connection, cancellationToken);
        return connection;
    }

    private static async Task<WorkRecordAggregateSqlRow?> BuildAggregateAsync(
        SqliteConnection connection,
        WorkRecordSqlRow? workRow)
    {
        if (workRow is null) return null;

        var restRows = await connection.QueryAsync<RestRecordSqlRow>(
            SelectRestsByWorkIdSql,
            new { WorkRecordId = workRow.Id });

        return new WorkRecordAggregateSqlRow
        {
            WorkRecord = workRow,
            RestRecords = [.. restRows]
        };
    }

    public async Task SaveAsync(
        WorkRecordSqlRow workRecord,
        IReadOnlyList<RestRecordSqlRow> restRecords,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(UpsertWorkSql, workRecord, transaction);

        await connection.ExecuteAsync(
            DeleteRestByWorkIdSql,
            new { WorkRecordId = workRecord.Id },
            transaction);

        if (restRecords.Count > 0)
        {
            await connection.ExecuteAsync(InsertRestSql, restRecords, transaction);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(DeleteWorkSql, new { Id = id });
    }

    public async Task<WorkRecordAggregateSqlRow?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var workRow = await connection.QuerySingleOrDefaultAsync<WorkRecordSqlRow>(SelectWorkByIdSql, new { Id = id });
        return await BuildAggregateAsync(connection, workRow);
    }

    public async Task<WorkRecordAggregateSqlRow?> GetByDateAsync(string workDate, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var workRow = await connection.QuerySingleOrDefaultAsync<WorkRecordSqlRow>(SelectWorkByDateSql, new { WorkDate = workDate });
        return await BuildAggregateAsync(connection, workRow);
    }

    public async Task<IReadOnlyList<WorkRecordAggregateSqlRow>> GetByMonthAsync(
        string monthStart,
        string monthEnd,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var workRows = await connection.QueryAsync<WorkRecordSqlRow>(
            SelectWorkByMonthSql,
            new { StartDate = monthStart, EndDate = monthEnd });

        if (!workRows.Any()) return [];

        var restRows = await connection.QueryAsync<RestRecordSqlRow>(
            SelectRestsByMonthSql,
            new { StartDate = monthStart, EndDate = monthEnd });

        return [.. workRows
            .Select(workRow => new WorkRecordAggregateSqlRow
            {
                WorkRecord = workRow,
                RestRecords = [.. restRows.Where(restRow => restRow.WorkRecordId == workRow.Id)]
            })];
    }
}
