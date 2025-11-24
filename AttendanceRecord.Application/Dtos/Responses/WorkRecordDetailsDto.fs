namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities

type WorkRecordDetailsDto =
    { Id: Guid
      Date: DateTime
      WorkTimeDuration: TimeDurationDto
      RestTimes: RestRecordDetailsDto list
      Summary: WorkRecordSummaryDto }

module WorkRecordDetailsDto =
    let fromDomain
        (now: DateTime)
        (standardWorkTime: TimeSpan)
        (workRecord: WorkRecord)
        : WorkRecordDetailsDto =
        { Id = workRecord.Id
          Date = workRecord |> WorkRecord.getDate
          WorkTimeDuration = workRecord.Duration |> TimeDurationDto.fromDomain now
          RestTimes = workRecord.RestRecords |> List.map (RestRecordDetailsDto.fromDomain now)
          Summary = workRecord |> WorkRecordSummaryDto.fromDomain now standardWorkTime }
