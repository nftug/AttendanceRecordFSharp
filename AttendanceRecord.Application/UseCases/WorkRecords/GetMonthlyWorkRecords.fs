namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses

type GetMonthlyWorkRecords =
    { Handle: DateTime -> TaskResult<WorkRecordListDto, string> }

module GetMonthlyWorkRecords =
    let private handle (repository: WorkRecordRepository) (getAppConfig: unit -> AppConfig) (monthDate: DateTime) =
        taskResult {
            let! monthlyRecords = repository.GetMonthly monthDate
            let now = DateTime.Now
            let standardWorkTime = getAppConfig().StandardWorkTime

            return monthlyRecords |> WorkRecordListDto.fromDomain monthDate now standardWorkTime
        }

    let create (repository: WorkRecordRepository) (getAppConfig: unit -> AppConfig) : GetMonthlyWorkRecords =
        { Handle = fun monthDate -> handle repository getAppConfig monthDate }
