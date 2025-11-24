namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities

type WorkRecordSummaryDto =
    { TotalWorkTime: TimeSpan
      TotalRestTime: TimeSpan
      Overtime: TimeSpan }

module WorkRecordSummaryDto =
    let empty: WorkRecordSummaryDto =
        { TotalWorkTime = TimeSpan.Zero
          TotalRestTime = TimeSpan.Zero
          Overtime = TimeSpan.Zero }

    let fromDomain (now: DateTime) (standardWorkTime: TimeSpan) (workRecord: WorkRecord) =
        { TotalWorkTime = workRecord |> WorkRecord.getDuration now
          TotalRestTime = workRecord |> WorkRecord.getRestDuration now
          Overtime = workRecord |> WorkRecord.getOvertimeDuration now standardWorkTime }
