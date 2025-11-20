namespace AttendanceRecord.Application.Dtos.Requests

open System
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects
open AttendanceRecord.Application.Dtos.Responses
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
    let empty (baseDate: DateTime) : RestRecordSaveRequestDto =
        { Id = None
          StartedAt = baseDate.Date
          EndedAt = None }

    let tryToDomain (dto: RestRecordSaveRequestDto) : Result<RestRecord, string> =
        TimeDuration.tryCreate dto.StartedAt dto.EndedAt
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
    let empty (baseDate: DateTime) : WorkRecordSaveRequestDto =
        { Id = None
          StartedAt = baseDate.Date
          EndedAt = None
          RestRecords = [] }

    let fromResponse (dto: WorkRecordDetailsDto) : WorkRecordSaveRequestDto =
        { Id = Some dto.Id
          StartedAt = dto.WorkTimeDuration.StartedAt
          EndedAt = dto.WorkTimeDuration.EndedAt
          RestRecords =
            dto.RestTimes
            |> List.map (fun rt ->
                { Id = Some rt.Id
                  StartedAt = rt.Duration.StartedAt
                  EndedAt = rt.Duration.EndedAt }) }
