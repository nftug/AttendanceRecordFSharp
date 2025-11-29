namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Services

type ToggleRest =
    { Handle: unit -> CancellationToken -> TaskResult<WorkStatusDto, WorkRecordError list> }

module ToggleRest =
    let private handle repository (workStatusStore: WorkStatusStore) (ct: CancellationToken) =
        taskResult {
            let now = DateTime.Now

            let! workTodayOption =
                repository.GetByDate now.Date ct |> TaskResult.mapError WorkRecordErrors.variant

            let! workToday =
                match workTodayOption with
                | Some record -> record |> WorkRecord.tryToggleRest now
                | None -> Error(WorkRecordErrors.variant "No work record found for today.")

            do! repository.Save workToday ct |> TaskResult.mapError WorkRecordErrors.variant
            do! workStatusStore.Reload() |> TaskResult.mapError WorkRecordErrors.variant

            return workStatusStore.WorkStatus.CurrentValue
        }

    let create (repository: WorkRecordRepository) (workStatusStore: WorkStatusStore) : ToggleRest =
        { Handle = fun () ct -> handle repository workStatusStore ct }
