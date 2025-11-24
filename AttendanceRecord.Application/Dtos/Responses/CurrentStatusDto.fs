namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.Entities

type CurrentStatusDto =
    { CurrentTime: DateTime
      Summary: WorkRecordSummaryDto
      OvertimeMonthly: TimeSpan
      IsActive: bool
      IsWorking: bool
      IsResting: bool }

module CurrentStatusDto =
    let getEmpty () : CurrentStatusDto =
        { CurrentTime = DateTime.Now
          Summary = WorkRecordSummaryDto.empty
          OvertimeMonthly = TimeSpan.Zero
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
              Summary = record |> WorkRecordSummaryDto.fromDomain now standardWorkTime
              OvertimeMonthly = monthlyOvertime
              IsActive = record |> WorkRecord.isActive now
              IsWorking = record |> WorkRecord.isWorking now
              IsResting = record |> WorkRecord.isResting now }
        | None ->
            { getEmpty () with
                OvertimeMonthly = monthlyOvertime }
