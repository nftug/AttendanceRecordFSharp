namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses

type GetWorkRecordDetails =
    { Handle: Guid -> CancellationToken -> TaskResult<WorkRecordDetailsDto option, string> }

module GetWorkRecordDetails =
    let private handle
        (repository: WorkRecordRepository)
        (getAppConfig: unit -> AppConfig)
        (workRecordId: Guid)
        (ct: CancellationToken)
        =
        taskResult {
            let! workRecordOption = repository.GetById workRecordId ct
            let standardWorkTime = getAppConfig().StandardWorkTime
            let now = DateTime.Now

            return
                workRecordOption
                |> Option.map (WorkRecordDetailsDto.fromDomain now standardWorkTime)
        }

    let create
        (repository: WorkRecordRepository)
        (getAppConfig: unit -> AppConfig)
        : GetWorkRecordDetails =
        { Handle = fun workRecordId ct -> handle repository getAppConfig workRecordId ct }
