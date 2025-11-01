namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities

type CurrentStatusDto =
    { CurrentTime: DateTime
      WorkDuration: TimeSpan
      RestDuration: TimeSpan
      OvertimeDuration: TimeSpan
      OvertimeMonthlyDuration: TimeSpan
      IsActive: bool
      IsWorking: bool
      IsResting: bool }

module CurrentStatusDto =
    let getEmpty () : CurrentStatusDto =
        { CurrentTime = DateTime.Now
          WorkDuration = TimeSpan.Zero
          RestDuration = TimeSpan.Zero
          OvertimeDuration = TimeSpan.Zero
          OvertimeMonthlyDuration = TimeSpan.Zero
          IsActive = false
          IsWorking = false
          IsResting = false }

    let fromDomain
        (standardWorkTime: TimeSpan)
        (monthlyOvertime: TimeSpan)
        (workRecord: WorkTimeRecord option)
        : CurrentStatusDto =
        match workRecord with
        | Some record ->
            { CurrentTime = DateTime.Now
              WorkDuration = WorkTimeRecord.getDuration record
              RestDuration = WorkTimeRecord.getRestDuration record
              OvertimeDuration = WorkTimeRecord.getOvertimeDuration standardWorkTime record
              OvertimeMonthlyDuration = monthlyOvertime
              IsActive = WorkTimeRecord.isActive record
              IsWorking = WorkTimeRecord.isWorking record
              IsResting = WorkTimeRecord.isResting record }
        | None ->
            { getEmpty () with
                OvertimeMonthlyDuration = monthlyOvertime }
