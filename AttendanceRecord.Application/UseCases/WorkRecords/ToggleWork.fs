namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Services

type ToggleWork =
    { Handle: unit -> CancellationToken -> TaskResult<WorkStatusDto, WorkRecordError list> }

module ToggleWork =
    let private handle repository (workStatusStore: WorkStatusStore) (ct: CancellationToken) =
        taskResult {
            let now = DateTime.Now

            let! workTodayOption =
                repository.GetByDate now.Date ct |> TaskResult.mapError WorkRecordErrors.variant

            let! workToday =
                match workTodayOption with
                | Some record -> record |> WorkRecord.tryToggleWork now
                | None -> WorkRecord.createStart () |> Ok

            do! repository.Save workToday ct |> TaskResult.mapError WorkRecordErrors.variant
            do! workStatusStore.Reload() |> TaskResult.mapError WorkRecordErrors.variant

            return workStatusStore.WorkStatus.CurrentValue
        }

    let create (repository: WorkRecordRepository) (workStatusStore: WorkStatusStore) : ToggleWork =
        { Handle = fun () ct -> handle repository workStatusStore ct }
