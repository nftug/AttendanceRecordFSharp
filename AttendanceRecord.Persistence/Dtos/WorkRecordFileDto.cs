namespace AttendanceRecord.Persistence.Dtos;

public record WorkRecordFileDto(Guid Id, TimeDurationFileDto Duration, IEnumerable<RestRecordFileDto> RestRecords);

public record RestRecordFileDto(Guid Id, TimeDurationFileDto Duration);

public record TimeDurationFileDto(DateTime StartedOn, DateTime? FinishedOn);
