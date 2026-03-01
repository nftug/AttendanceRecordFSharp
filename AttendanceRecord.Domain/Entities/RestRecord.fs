namespace AttendanceRecord.Domain.Entities

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.ValueObjects
open AttendanceRecord.Domain.Errors

type RestVariant =
   | RegularRest
   | PaidRest

type RestRecord =
   { Id: Guid
     Duration: TimeDuration
     Variant: RestVariant }

module RestRecord =
   // --- Accessors ---
   let getDuration (now: DateTime) (record: RestRecord) : TimeSpan =
      record.Duration |> TimeDuration.getDuration now

   let isActive (now: DateTime) (record: RestRecord) : bool =
      record.Duration |> TimeDuration.isActive now

   let getStartedAt (record: RestRecord) : DateTime =
      record.Duration |> TimeDuration.getStartedAt

   let getEndedAt (record: RestRecord) : DateTime option =
      record.Duration |> TimeDuration.getEndedAt

   let getDate (record: RestRecord) : DateTime = getStartedAt record |> _.Date

   // -- Factory methods ---
   let hydrate (id: Guid) (variant: RestVariant) (duration: TimeDuration) : RestRecord =
      { Id = id
        Duration = duration
        Variant = variant }

   let create (id: Guid) (variant: RestVariant) (duration: TimeDuration) : RestRecord =
      { Id = id
        Duration = duration
        Variant = variant }

   let createStart () : RestRecord =
      create (Guid.NewGuid()) RegularRest (TimeDuration.createStart ())

   // --- State transitions ---
   let tryCreateEnd (now: DateTime) (record: RestRecord) : Result<RestRecord, RestRecordError> =
      match record.Duration |> TimeDuration.isActive now with
      | true ->
         TimeDuration.tryCreateEnd record.Duration
         |> Result.mapError (fun e -> RestDurationError(record.Id, e))
         |> Result.map (fun duration -> { record with Duration = duration })
      | false -> Error(RestGenericError(record.Id, "開始されていない休憩記録は終了できません。"))
