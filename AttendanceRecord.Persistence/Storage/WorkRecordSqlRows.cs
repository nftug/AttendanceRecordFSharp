namespace AttendanceRecord.Persistence.Storage;

public sealed class WorkRecordSqlRow
{
    public string Id { get; set; } = string.Empty;
    public string StartedAt { get; set; } = string.Empty;
    public string? EndedAt { get; set; }
    public string WorkDate { get; set; } = string.Empty;
}

public sealed class RestRecordSqlRow
{
    public string Id { get; set; } = string.Empty;
    public string WorkRecordId { get; set; } = string.Empty;
    public string StartedAt { get; set; } = string.Empty;
    public string? EndedAt { get; set; }
    public long Variant { get; set; }
}

public sealed class WorkRecordAggregateSqlRow
{
    public required WorkRecordSqlRow WorkRecord { get; init; }
    public required IReadOnlyList<RestRecordSqlRow> RestRecords { get; init; }
}