namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Services

type DeleteWorkRecord =
    { Handle: Guid -> TaskResult<unit, string> }

module DeleteWorkRecord =
    let private handle (repository: WorkRecordRepository) (currentStatusStore: CurrentStatusStore) (id: Guid) =
        taskResult {
            let! existingWorkRecordOption = repository.GetById id

            if existingWorkRecordOption.IsNone then
                return! Error $"Work record with ID {id} not found."

            do! repository.Delete id
            do! currentStatusStore.Reload()
        }

    let create (repository: WorkRecordRepository) (currentStatusStore: CurrentStatusStore) : DeleteWorkRecord =
        { Handle = fun id -> handle repository currentStatusStore id }
