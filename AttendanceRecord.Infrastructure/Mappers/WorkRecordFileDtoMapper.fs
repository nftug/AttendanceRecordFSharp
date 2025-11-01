namespace AttendanceRecord.Infrastructure.Mappers

open System.Collections.Generic
open AttendanceRecord.Persistence.Dtos
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects

module private TimeDurationFileDtoMapper =
    let fromDomain (duration: TimeDuration) : TimeDurationFileDto =
        TimeDurationFileDto(
            duration |> TimeDuration.getStartedAt,
            duration |> TimeDuration.getEndedAt |> Option.toNullable
        )

    let toDomain (dto: TimeDurationFileDto) : TimeDuration =
        TimeDuration.hydrate dto.StartedOn (dto.FinishedOn |> Option.ofNullable)

module WorkRecordFileDtoMapper =
    let fromDomain (records: WorkRecord list) : IEnumerable<WorkRecordFileDto> =
        records
        |> WorkRecord.getSortedList
        |> Seq.map (fun r ->
            WorkRecordFileDto(
                r.Id,
                TimeDurationFileDtoMapper.fromDomain r.Duration,
                r.RestTimes
                |> Seq.map (fun r -> RestRecordFileDto(r.Id, TimeDurationFileDtoMapper.fromDomain r.Duration))
            ))

    let toDomain (dtos: IEnumerable<WorkRecordFileDto>) : WorkRecord list =
        dtos
        |> Seq.map (fun dto ->
            WorkRecord.hydrate
                dto.Id
                (TimeDurationFileDtoMapper.toDomain dto.Duration)
                (dto.RestRecords
                 |> Seq.map (fun r -> RestRecord.hydrate r.Id (TimeDurationFileDtoMapper.toDomain r.Duration))
                 |> Seq.toList))
        |> Seq.toList
        |> WorkRecord.getSortedList
