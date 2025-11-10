namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Services

type ToggleRest =
    { Handle: unit -> CancellationToken -> TaskResult<CurrentStatusDto, string> }

module ToggleRest =
    let private handle repository (currentStatusStore: CurrentStatusStore) (ct: CancellationToken) =
        taskResult {
            let now = DateTime.Now
            let! workTodayOption = repository.GetByDate now.Date ct

            let! workToday =
                match workTodayOption with
                | Some record -> record |> WorkRecord.tryToggleRest now
                | None -> Error "No work record for today to toggle rest."

            do! repository.Save workToday ct
            do! currentStatusStore.Reload()

            return currentStatusStore.CurrentStatus.CurrentValue
        }

    let create (repository: WorkRecordRepository) (currentStatusStore: CurrentStatusStore) : ToggleRest =
        { Handle = fun () ct -> handle repository currentStatusStore ct }
