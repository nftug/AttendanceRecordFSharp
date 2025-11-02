namespace AttendanceRecord.Application.Dtos.Requests

open System
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects
open FsToolkit.ErrorHandling

type RestRecordSaveRequestDto =
    { Id: Guid option
      StartedAt: DateTime
      EndedAt: DateTime option }

type WorkRecordSaveRequestDto =
    { Id: Guid option
      StartedAt: DateTime
      EndedAt: DateTime option
      RestRecords: RestRecordSaveRequestDto list }

module RestRecordSaveRequestDto =
    let tryToDomain (dto: RestRecordSaveRequestDto) : Result<RestRecord, string> =
        TimeDuration.create dto.StartedAt dto.EndedAt
        |> Result.map (fun duration ->
            match dto.Id with
            | Some id -> RestRecord.hydrate id duration
            | None -> RestRecord.create duration)

    let tryToDomainOfList (dtos: RestRecordSaveRequestDto list) : Result<RestRecord list, string> =
        dtos
        |> List.map tryToDomain
        |> List.sequenceResultA
        |> Result.mapError (String.concat "; ")

module WorkRecordSaveRequestDto =
    let tryToDomain (dto: WorkRecordSaveRequestDto) : Result<WorkRecord, string> =
        result {
            let! restRecords =
                dto.RestRecords
                |> List.map RestRecordSaveRequestDto.tryToDomain
                |> List.sequenceResultA
                |> Result.mapError (String.concat "; ")

            let! duration = TimeDuration.create dto.StartedAt dto.EndedAt

            return
                match dto.Id with
                | Some id -> WorkRecord.hydrate id duration restRecords
                | None -> WorkRecord.create duration restRecords
        }
