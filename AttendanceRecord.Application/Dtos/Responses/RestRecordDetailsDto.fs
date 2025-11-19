namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities

type RestRecordDetailsDto =
    { Id: Guid
      Date: DateTime
      Duration: TimeDurationDto
      IsActive: bool }

module RestRecordDetailsDto =
    let empty: RestRecordDetailsDto =
        { Id = Guid.Empty
          Date = DateTime.MinValue
          Duration = TimeDurationDto.empty
          IsActive = false }

    let fromDomain (now: DateTime) (restRecord: RestRecord) : RestRecordDetailsDto =
        { Id = restRecord.Id
          Date = restRecord |> RestRecord.getDate
          Duration = restRecord.Duration |> TimeDurationDto.fromDomain now
          IsActive = restRecord |> RestRecord.isActive now }
