namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Dtos.Responses

type GetWorkRecordDetails =
    { Handle: Guid -> TaskResult<WorkRecordDetailsDto option, string> }

module GetWorkRecordDetails =
    let private handle (repository: WorkRecordRepository) (standardWorkTime: TimeSpan) (workRecordId: Guid) =
        taskResult {
            let! workRecordOption = repository.GetById workRecordId

            return
                workRecordOption
                |> Option.map (WorkRecordDetailsDto.fromDomain DateTime.Now standardWorkTime)
        }

    let create (repository: WorkRecordRepository) (standardWorkTime: TimeSpan) : GetWorkRecordDetails =
        { Handle = fun workRecordId -> handle repository standardWorkTime workRecordId }
