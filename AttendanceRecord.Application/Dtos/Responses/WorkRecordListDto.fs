namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities

type WorkRecordListItemDto =
    { Id: Guid
      Date: DateTime
      HasUnfinishedWarning: bool }

type WorkRecordListDto =
    { MonthDate: DateTime
      WorkRecords: WorkRecordListItemDto list
      WorkTimeTotal: TimeSpan
      RestTimeTotal: TimeSpan
      OvertimeTotal: TimeSpan }

module WorkRecordListItemDto =
    let fromDomain (now: DateTime) (workRecord: WorkRecord) : WorkRecordListItemDto =
        { Id = workRecord.Id
          Date = workRecord |> WorkRecord.getDate
          HasUnfinishedWarning = workRecord |> WorkRecord.hasUnfinishedWarning now }

module WorkRecordListDto =
    let empty (now: DateTime) : WorkRecordListDto =
        { MonthDate = DateTime(now.Year, now.Month, 1)
          WorkRecords = []
          WorkTimeTotal = TimeSpan.Zero
          RestTimeTotal = TimeSpan.Zero
          OvertimeTotal = TimeSpan.Zero }

    let fromDomain
        (monthDate: DateTime)
        (now: DateTime)
        (standardWorkTime: TimeSpan)
        (workRecords: WorkRecord list)
        : WorkRecordListDto =
        { MonthDate = monthDate
          WorkRecords = workRecords |> List.map (WorkRecordListItemDto.fromDomain now)
          WorkTimeTotal = workRecords |> WorkRecordTally.getWorkTimeTotal now
          RestTimeTotal = workRecords |> WorkRecordTally.getRestTimeTotal now
          OvertimeTotal = workRecords |> WorkRecordTally.getOvertimeTotal now standardWorkTime }
