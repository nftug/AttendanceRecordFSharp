namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Services

type ToggleRest =
    { Handle: unit -> TaskResult<CurrentStatusDto, string> }

module ToggleRest =
    let private handle repository (currentStatusStore: CurrentStatusStore) =
        taskResult {
            let now = DateTime.Now
            let! workTodayOption = repository.GetByDate now.Date

            let! workToday =
                match workTodayOption with
                | Some record -> record |> WorkRecord.tryToggleRest now
                | None -> Error "No work record for today to toggle rest."

            do! repository.Save workToday
            do! currentStatusStore.Reload()

            return currentStatusStore.CurrentStatus.CurrentValue
        }

    let create (repository: WorkRecordRepository) (currentStatusStore: CurrentStatusStore) : ToggleRest =
        { Handle = fun () -> handle repository currentStatusStore }
