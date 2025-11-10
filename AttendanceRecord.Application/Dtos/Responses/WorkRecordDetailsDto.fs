namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities

type WorkRecordDetailsDto =
    { Id: Guid
      Date: DateTime
      WorkTimeDuration: TimeDurationDto
      RestTimes: RestRecordDetailsDto list
      WorkTime: TimeSpan
      RestTime: TimeSpan
      Overtime: TimeSpan
      IsActive: bool
      IsWorking: bool
      IsResting: bool }

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
          WorkTime = workRecord |> WorkRecord.getDuration now
          RestTime = workRecord |> WorkRecord.getRestDuration now
          Overtime = workRecord |> WorkRecord.getOvertimeDuration now standardWorkTime
          IsActive = workRecord |> WorkRecord.isActive now
          IsWorking = workRecord |> WorkRecord.isWorking now
          IsResting = workRecord |> WorkRecord.isResting now }
