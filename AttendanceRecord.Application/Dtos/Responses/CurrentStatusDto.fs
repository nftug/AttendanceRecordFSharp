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
        (now: DateTime)
        (standardWorkTime: TimeSpan)
        (workRecord: WorkRecord option)
        (monthlyOvertime: TimeSpan)
        : CurrentStatusDto =
        match workRecord with
        | Some record ->
            { CurrentTime = now
              WorkDuration = record |> WorkRecord.getDuration now
              RestDuration = record |> WorkRecord.getRestDuration now
              OvertimeDuration = record |> WorkRecord.getOvertimeDuration now standardWorkTime
              OvertimeMonthlyDuration = monthlyOvertime
              IsActive = record |> WorkRecord.isActive now
              IsWorking = record |> WorkRecord.isWorking now
              IsResting = record |> WorkRecord.isResting now }
        | None ->
            { getEmpty () with
                OvertimeMonthlyDuration = monthlyOvertime }
