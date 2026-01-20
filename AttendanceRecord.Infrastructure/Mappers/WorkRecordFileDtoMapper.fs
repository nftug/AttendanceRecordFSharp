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
            r.RestRecords
            |> Seq.map (fun r ->
               RestRecordFileDto(
                  r.Id,
                  TimeDurationFileDtoMapper.fromDomain r.Duration,
                  match r.Variant with
                  | RegularRest -> RestVariantEnum.RegularRest
                  | PaidRest -> RestVariantEnum.PaidRest
               ))
         ))

   let toDomain (dtos: IEnumerable<WorkRecordFileDto>) : WorkRecord list =
      let toRestRecord (dto: RestRecordFileDto) : RestRecord option =
         match Option.ofObj dto with
         | None -> None
         | Some dto ->
            dto.Duration
            |> Option.ofObj
            |> Option.map (fun duration ->
               RestRecord.hydrate
                  dto.Id
                  (match dto.Variant with
                   | RestVariantEnum.RegularRest -> RegularRest
                   | RestVariantEnum.PaidRest -> PaidRest
                   | _ -> RegularRest)
                  (TimeDurationFileDtoMapper.toDomain duration))

      let toWorkRecord (dto: WorkRecordFileDto) : WorkRecord option =
         match Option.ofObj dto with
         | None -> None
         | Some dto ->
            dto.Duration
            |> Option.ofObj
            |> Option.map (fun duration ->
               let restRecords =
                  dto.RestRecords
                  |> Option.ofObj
                  |> Option.defaultValue Seq.empty
                  |> Seq.choose toRestRecord
                  |> Seq.toList

               WorkRecord.hydrate dto.Id (TimeDurationFileDtoMapper.toDomain duration) restRecords)

      dtos
      |> Option.ofObj
      |> Option.defaultValue Seq.empty
      |> Seq.choose toWorkRecord
      |> Seq.toList
      |> WorkRecord.getSortedList
