namespace AttendanceRecord.Domain.Entities

open System

module WorkRecordTally =
    let getWorkTimeTotal (now: DateTime) (workRecords: WorkRecord list) : TimeSpan =
        workRecords
        |> List.sumBy (WorkRecord.getDuration now >> _.Ticks)
        |> TimeSpan.FromTicks

    let getRestTimeTotal (now: DateTime) (workRecords: WorkRecord list) : TimeSpan =
        workRecords
        |> List.sumBy (WorkRecord.getRestDuration now >> _.Ticks)
        |> TimeSpan.FromTicks

    let getOvertimeTotal (now: DateTime) (standardWorkTime: TimeSpan) (workRecords: WorkRecord list)  : TimeSpan =
        workRecords
        |> List.sumBy (WorkRecord.getOvertimeDuration now standardWorkTime >> _.Ticks)
        |> TimeSpan.FromTicks
