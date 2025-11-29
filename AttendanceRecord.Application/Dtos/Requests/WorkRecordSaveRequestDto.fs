namespace AttendanceRecord.Application.Dtos.Requests

open System
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Domain.ValueObjects
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Enums
open FsToolkit.ErrorHandling

type RestRecordSaveRequestDto =
    { Id: Guid
      StartedAt: DateTime
      EndedAt: DateTime option
      Variant: RestVariantEnum }

type WorkRecordSaveRequestDto =
    { Id: Guid option
      StartedAt: DateTime
      EndedAt: DateTime option
      RestRecords: RestRecordSaveRequestDto list }

module RestRecordSaveRequestDto =
    let empty (baseDate: DateTime) : RestRecordSaveRequestDto =
        { Id = Guid.NewGuid()
          StartedAt = baseDate.Date
          EndedAt = None
          Variant = RestVariantEnum.RegularRest }

    let tryToDomain (dto: RestRecordSaveRequestDto) : Result<RestRecord, RestRecordError> =
        TimeDuration.tryCreate dto.StartedAt dto.EndedAt
        |> Result.mapError (fun e -> RestDurationError(dto.Id, e))
        |> Result.map (RestRecord.create dto.Id (RestVariantEnum.toDomain dto.Variant))

    let tryToDomainOfList
        (dtos: RestRecordSaveRequestDto list)
        : Result<RestRecord list, RestRecordError list> =
        dtos |> List.map tryToDomain |> List.sequenceResultA

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
                { Id = rt.Id
                  StartedAt = rt.Duration.StartedAt
                  EndedAt = rt.Duration.EndedAt
                  Variant = rt.Variant }) }
