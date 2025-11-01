namespace AttendanceRecord.Domain.Entities

open System

module WorkTimeTally =
    let getWorkTimeTotal (workRecords: WorkTimeRecord list) : TimeSpan =
        workRecords
        |> List.sumBy (WorkTimeRecord.getDuration >> _.Ticks)
        |> TimeSpan.FromTicks

    let getRestTimeTotal (workRecords: WorkTimeRecord list) : TimeSpan =
        workRecords
        |> List.sumBy (WorkTimeRecord.getRestDuration >> _.Ticks)
        |> TimeSpan.FromTicks

    let getOvertimeTotal (workRecords: WorkTimeRecord list) (standardWorkTime: TimeSpan) : TimeSpan =
        workRecords
        |> List.sumBy (WorkTimeRecord.getOvertimeDuration standardWorkTime >> _.Ticks)
        |> TimeSpan.FromTicks
