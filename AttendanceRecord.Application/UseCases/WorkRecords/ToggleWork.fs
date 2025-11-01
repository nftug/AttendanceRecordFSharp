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
    let private handle (repository: WorkRecordRepository) (currentStatusStore: CurrentStatusStore) =
        taskResult {
            let! workTodayOption = repository.GetByDate DateTime.Now.Date

            let! workToday =
                match workTodayOption with
                | Some record -> WorkTimeRecord.toggleWork record
                | None -> Ok(WorkTimeRecord.createStart ())

            do! repository.Save workToday
            do! currentStatusStore.Reload()

            return currentStatusStore.CurrentStatus.CurrentValue
        }

    let create (repository: WorkRecordRepository) (currentStatusStore: CurrentStatusStore) : ToggleWork =
        { Handle = fun () -> handle repository currentStatusStore }
