namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Services

type ToggleWork =
    { Handle: unit -> TaskResult<CurrentStatusDto, string> }

module ToggleWork =
    let private handle repository (currentStatusStore: CurrentStatusStore) =
        taskResult {
            let now = DateTime.Now
            let! workTodayOption = repository.GetByDate now.Date

            let! workToday =
                match workTodayOption with
                | Some record -> record |> WorkRecord.toggleWork now
                | None -> Ok(WorkRecord.createStart ())

            do! repository.Save workToday
            do! currentStatusStore.Reload()

            return currentStatusStore.CurrentStatus.CurrentValue
        }

    let create (repository: WorkRecordRepository) (currentStatusStore: CurrentStatusStore) : ToggleWork =
        { Handle = fun () -> handle repository currentStatusStore }
