namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities

type WorkRecordListItemDto = { Id: Guid; Date: DateTime }

type WorkRecordListDto =
    { MonthDate: DateTime
      WorkRecords: WorkRecordListItemDto list
      WorkTimeTotal: TimeSpan
      RestTimeTotal: TimeSpan
      OvertimeTotal: TimeSpan }

module WorkRecordListItemDto =
    let fromDomain (workRecord: WorkRecord) : WorkRecordListItemDto =
        { Id = workRecord.Id
          Date = workRecord |> WorkRecord.getDate }

module WorkRecordListDto =
    let fromDomain
        (monthDate: DateTime)
        (now: DateTime)
        (standardWorkTime: TimeSpan)
        (workRecords: WorkRecord list)
        : WorkRecordListDto =
        { MonthDate = monthDate
          WorkRecords = workRecords |> List.map WorkRecordListItemDto.fromDomain
          WorkTimeTotal = workRecords |> WorkRecordTally.getWorkTimeTotal now
          RestTimeTotal = workRecords |> WorkRecordTally.getRestTimeTotal now
          OvertimeTotal = workRecords |> WorkRecordTally.getOvertimeTotal now standardWorkTime }
