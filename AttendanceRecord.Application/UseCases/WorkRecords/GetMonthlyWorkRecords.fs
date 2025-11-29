namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses

type GetMonthlyWorkRecords =
    { Handle: DateTime -> CancellationToken -> TaskResult<WorkRecordListDto, string> }

module GetMonthlyWorkRecords =
    let private handle
        (repository: WorkRecordRepository)
        (getAppConfig: unit -> AppConfig)
        (monthDate: DateTime)
        (ct: CancellationToken)
        =
        taskResult {
            let! monthlyRecords = repository.GetMonthly monthDate ct
            let now = DateTime.Now
            let standardWorkTime = getAppConfig().StandardWorkTime |> StandardWorkTime.value

            return monthlyRecords |> WorkRecordListDto.fromDomain monthDate now standardWorkTime
        }

    let create
        (repository: WorkRecordRepository)
        (getAppConfig: unit -> AppConfig)
        : GetMonthlyWorkRecords =
        { Handle = fun monthDate ct -> handle repository getAppConfig monthDate ct }
