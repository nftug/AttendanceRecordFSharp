namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses

type GetMonthlyWorkRecords =
    { Handle: DateTime -> TaskResult<WorkRecordListDto, string> }

module GetMonthlyWorkRecords =
    let private handle (repository: WorkRecordRepository) (standardWorkTime: TimeSpan) (monthDate: DateTime) =
        taskResult {
            let! monthlyRecords = repository.GetMonthly monthDate

            return
                monthlyRecords
                |> WorkRecordListDto.fromDomain monthDate DateTime.Now standardWorkTime
        }

    let create (repository: WorkRecordRepository) (standardWorkTime: TimeSpan) : GetMonthlyWorkRecords =
        { Handle = fun monthDate -> handle repository standardWorkTime monthDate }
