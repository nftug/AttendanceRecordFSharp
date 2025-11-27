using System.Text.Json.Serialization;

namespace AttendanceRecord.Persistence.Dtos;

public record WorkRecordFileDto(Guid Id, TimeDurationFileDto Duration, IEnumerable<RestRecordFileDto> RestRecords);

public record RestRecordFileDto(Guid Id, TimeDurationFileDto Duration, RestVariantEnum Variant);

public record TimeDurationFileDto(DateTime StartedOn, DateTime? FinishedOn);

[JsonConverter(typeof(JsonStringEnumConverter<RestVariantEnum>))]
public enum RestVariantEnum
{
    RegularRest = 0,
    PaidRest = 1
}
