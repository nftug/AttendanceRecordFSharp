namespace AttendanceRecord.Domain.Entities

open System

module WorkRecordTally =
    let getWorkTimeTotal (workRecords: WorkRecord list) : TimeSpan =
        workRecords
        |> List.sumBy (WorkRecord.getDuration >> _.Ticks)
        |> TimeSpan.FromTicks

    let getRestTimeTotal (workRecords: WorkRecord list) : TimeSpan =
        workRecords
        |> List.sumBy (WorkRecord.getRestDuration >> _.Ticks)
        |> TimeSpan.FromTicks

    let getOvertimeTotal (workRecords: WorkRecord list) (standardWorkTime: TimeSpan) : TimeSpan =
        workRecords
        |> List.sumBy (WorkRecord.getOvertimeDuration standardWorkTime >> _.Ticks)
        |> TimeSpan.FromTicks
