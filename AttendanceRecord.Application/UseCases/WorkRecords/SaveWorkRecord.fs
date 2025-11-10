namespace AttendanceRecord.Application.UseCases.WorkRecords

open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.Services
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects

type SaveWorkRecord =
    { Handle: WorkRecordSaveRequestDto -> CancellationToken -> TaskResult<unit, string> }

module SaveWorkRecord =
    let private handle
        (repository: WorkRecordRepository)
        (currentStatusStore: CurrentStatusStore)
        (request: WorkRecordSaveRequestDto)
        (ct: CancellationToken)
        =
        taskResult {
            let! restRecords = request.RestRecords |> RestRecordSaveRequestDto.tryToDomainOfList
            let! duration = TimeDuration.tryCreate request.StartedAt request.EndedAt

            let! workRecord =
                match request.Id with
                | Some id ->
                    taskResult {
                        let! recordOption = repository.GetById id ct

                        let! record =
                            recordOption
                            |> Result.requireSome $"Work record with ID {id} not found."

                        return! WorkRecord.tryUpdate duration restRecords record
                    }
                | None -> taskResult { return! WorkRecord.tryCreate duration restRecords }

            let! existingWorkRecord = repository.GetByDate (WorkRecord.getDate workRecord) ct

            match existingWorkRecord with
            | Some existingRecord when existingRecord.Id <> workRecord.Id ->
                return! Error "A work record for the specified date already exists."
            | _ -> ()

            do! repository.Save workRecord ct
            do! currentStatusStore.Reload()
        }

    let create
        (repository: WorkRecordRepository)
        (currentStatusStore: CurrentStatusStore)
        : SaveWorkRecord =
        { Handle = fun request ct -> handle repository currentStatusStore request ct }
