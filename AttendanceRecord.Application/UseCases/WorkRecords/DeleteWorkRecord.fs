namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Services

type DeleteWorkRecord =
    { Handle: Guid -> CancellationToken -> TaskResult<unit, string> }

module DeleteWorkRecord =
    let private handle
        (repository: WorkRecordRepository)
        (currentStatusStore: CurrentStatusStore)
        (id: Guid)
        (ct: CancellationToken)
        =
        taskResult {
            let! existingWorkRecordOption = repository.GetById id ct

            if existingWorkRecordOption.IsNone then
                return! Error $"Work record with ID {id} not found."

            do! repository.Delete id ct
            do! currentStatusStore.Reload()
        }

    let create (repository: WorkRecordRepository) (currentStatusStore: CurrentStatusStore) : DeleteWorkRecord =
        { Handle = fun id ct -> handle repository currentStatusStore id ct }
