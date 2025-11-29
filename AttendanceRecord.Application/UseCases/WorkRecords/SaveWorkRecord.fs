namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.Services
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects
open AttendanceRecord.Domain.Errors

type SaveWorkRecord =
    { Handle:
        WorkRecordSaveRequestDto -> CancellationToken -> TaskResult<Guid, WorkRecordError list> }

module SaveWorkRecord =
    let private handle
        (repository: WorkRecordRepository)
        (workStatusStore: WorkStatusStore)
        (request: WorkRecordSaveRequestDto)
        (ct: CancellationToken)
        =
        taskResult {
            let! restRecords =
                request.RestRecords
                |> RestRecordSaveRequestDto.tryToDomainOfList
                |> Result.mapError WorkRecordErrors.restList

            let! duration =
                TimeDuration.tryCreate request.StartedAt request.EndedAt
                |> Result.mapError WorkRecordErrors.duration

            let! workRecord =
                match request.Id with
                | Some id ->
                    taskResult {
                        let! recordOption =
                            repository.GetById id ct |> TaskResult.mapError WorkRecordErrors.variant

                        let! record =
                            recordOption
                            |> Result.requireSome (WorkRecordErrors.variant "勤務記録が見つかりません。")

                        return! WorkRecord.tryUpdate duration restRecords record
                    }
                | None -> taskResult { return! WorkRecord.tryCreate duration restRecords }

            let! existingRecordOption =
                repository.GetByDate (WorkRecord.getDate workRecord) ct
                |> TaskResult.mapError WorkRecordErrors.variant

            do!
                match existingRecordOption with
                | Some existingRecord when existingRecord.Id <> workRecord.Id ->
                    TaskResult.error (WorkRecordErrors.variant "指定した日付の勤務は既に登録されています。")
                | _ -> TaskResult.ok ()

            do! repository.Save workRecord ct |> TaskResult.mapError WorkRecordErrors.variant
            do! workStatusStore.Reload() |> TaskResult.mapError WorkRecordErrors.variant

            return workRecord.Id
        }

    let create
        (repository: WorkRecordRepository)
        (workStatusStore: WorkStatusStore)
        : SaveWorkRecord =
        { Handle = fun request ct -> handle repository workStatusStore request ct }
