namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities

type WorkRecordSummaryDto =
   { TotalWorkTime: TimeSpan
     TotalRestTime: TimeSpan
     TotalPaidRestTime: TimeSpan
     Overtime: TimeSpan }

module WorkRecordSummaryDto =
   let empty: WorkRecordSummaryDto =
      { TotalWorkTime = TimeSpan.Zero
        TotalRestTime = TimeSpan.Zero
        TotalPaidRestTime = TimeSpan.Zero
        Overtime = TimeSpan.Zero }

   let fromDomain (now: DateTime) (standardWorkTime: TimeSpan) (workRecord: WorkRecord) =
      { TotalWorkTime = workRecord |> WorkRecord.getWorkDuration now
        TotalRestTime = workRecord |> WorkRecord.getRestDuration now RegularRest
        TotalPaidRestTime = workRecord |> WorkRecord.getRestDuration now PaidRest
        Overtime = workRecord |> WorkRecord.getOvertimeDuration now standardWorkTime }
