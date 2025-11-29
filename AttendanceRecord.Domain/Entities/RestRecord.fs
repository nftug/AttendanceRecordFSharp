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
    // Factory methods
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

    let tryCreateEnd (now: DateTime) (record: RestRecord) : Result<RestRecord, RestRecordError> =
        match record.Duration |> TimeDuration.isActive now with
        | true ->
            TimeDuration.tryCreateEnd record.Duration
            |> Result.mapError (fun e -> RestDurationError(record.Id, e))
            |> Result.map (fun duration -> { record with Duration = duration })
        | false -> Error(RestVariantError(record.Id, "開始されていない休憩記録は終了できません。"))

    // Status getters
    let getDuration (now: DateTime) (record: RestRecord) : TimeSpan =
        record.Duration |> TimeDuration.getDuration now

    let isActive (now: DateTime) (record: RestRecord) : bool =
        record.Duration |> TimeDuration.isActive now

    let getStartedAt (record: RestRecord) : DateTime =
        record.Duration |> TimeDuration.getStartedAt

    let getEndedAt (record: RestRecord) : DateTime option =
        record.Duration |> TimeDuration.getEndedAt

    let getDate (record: RestRecord) : DateTime = getStartedAt record |> _.Date

    // List operations
    let getSortedList (records: RestRecord list) : RestRecord list =
        records |> List.sortBy getStartedAt

    let isRestingOfList (now: DateTime) (records: RestRecord list) : bool =
        records |> List.tryFind (isActive now) |> Option.isSome

    let getDurationOfList (now: DateTime) (records: RestRecord list) : TimeSpan =
        records |> List.sumBy (getDuration now >> _.Ticks) |> TimeSpan.FromTicks

    let addToList (record: RestRecord) (records: RestRecord list) : RestRecord list =
        records
        |> List.filter (fun r -> r.Id <> record.Id)
        |> List.append [ record ]
        |> getSortedList

    let startOfList (records: RestRecord list) : RestRecord list =
        records |> addToList (createStart ())

    let finishOfList
        (now: DateTime)
        (records: RestRecord list)
        : Result<RestRecord list, RestRecordError> =
        result {
            match records |> List.filter (isActive now) |> List.tryLast with
            | Some lastActive ->
                let! endedRest = lastActive |> tryCreateEnd now
                return records |> addToList endedRest
            | None -> return records
        }

    let toggleOfList
        (now: DateTime)
        (records: RestRecord list)
        : Result<RestRecord list, RestRecordError> =
        match records |> isRestingOfList now with
        | true -> records |> finishOfList now
        | false -> Ok(startOfList records)
