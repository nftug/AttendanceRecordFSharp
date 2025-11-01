namespace AttendanceRecord.Infrastructure.Mappers

open System.Collections.Generic
open AttendanceRecord.Persistence.Dtos
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects

module WorkRecordFileDtoMapper =
    let fromDomain (records: WorkTimeRecord list) : IEnumerable<WorkRecordFileDto> =
        records
        |> Seq.map (fun r ->
            WorkRecordFileDto(
                r.Id,
                TimeDurationFileDto(
                    r |> WorkTimeRecord.getStartedAt,
                    r |> WorkTimeRecord.getEndedAt |> Option.toNullable
                ),
                r.RestTimes
                |> Seq.map (fun rest ->
                    RestRecordFileDto(
                        rest.Id,
                        TimeDurationFileDto(
                            rest |> RestTimeRecord.getStartedAt,
                            rest |> RestTimeRecord.getEndedAt |> Option.toNullable
                        )
                    ))
            ))

    let toDomain (dtos: IEnumerable<WorkRecordFileDto>) : WorkTimeRecord list =
        dtos
        |> Seq.map (fun dto ->
            WorkTimeRecord.hydrate
                dto.Id
                (TimeDuration.hydrate dto.Duration.StartedOn (dto.Duration.FinishedOn |> Option.ofNullable))
                (dto.RestRecords
                 |> Seq.map (fun rDto ->
                     RestTimeRecord.hydrate
                         rDto.Id
                         (TimeDuration.hydrate rDto.Duration.StartedOn (rDto.Duration.FinishedOn |> Option.ofNullable)))
                 |> Seq.toList))
        |> Seq.toList
