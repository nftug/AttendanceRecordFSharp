namespace AttendanceRecord.Domain.Entities

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.ValueObjects

type RestRecord = { Id: Guid; Duration: TimeDuration }

module RestRecord =
    // Factory methods
    let hydrate (id: Guid) (duration: TimeDuration) : RestRecord = { Id = id; Duration = duration }

    let create (duration: TimeDuration) : RestRecord =
        { Id = Guid.NewGuid()
          Duration = duration }

    let createStart () : RestRecord = create (TimeDuration.createStart ())

    let tryCreateEnd (now: DateTime) (record: RestRecord) : Result<RestRecord, string> =
        match record.Duration |> TimeDuration.isActive now  with
        | true ->
            TimeDuration.createEnd record.Duration
            |> Result.map (fun duration -> { record with Duration = duration })
        | false -> Error "Rest time is not active"

    // Status getters
    let getDuration (now: DateTime) (record: RestRecord) : TimeSpan =
        record.Duration |> TimeDuration.getDuration now

    let isActive (now: DateTime) (record: RestRecord) : bool =
        record.Duration |> TimeDuration.isActive now

    let getStartedAt (record: RestRecord) : DateTime =
        record.Duration |> TimeDuration.getStartedAt

    let getEndedAt (record: RestRecord) : DateTime option =
        record.Duration |> TimeDuration.getEndedAt

    let getDate (record: RestRecord) : DateTime =
        getStartedAt record |> fun dt -> dt.Date

    // List operations
    let getSortedList (records: RestRecord list) : RestRecord list =
        records |> List.sortBy getStartedAt

    let isRestingOfList (now: DateTime) (records: RestRecord list) : bool =
        records |> List.tryFind (isActive now) |> Option.isSome

    let getDurationOfList (now: DateTime) (records: RestRecord list) : TimeSpan =
        records
        |> List.sumBy (getDuration now >> _.Ticks)
        |> TimeSpan.FromTicks

    let addToList (record: RestRecord) (records: RestRecord list) : RestRecord list =
        records
        |> List.filter (fun r -> r.Id <> record.Id)
        |> List.append [ record ]
        |> getSortedList

    let startOfList (records: RestRecord list) : RestRecord list = records |> addToList (createStart ())

    let finishOfList (now: DateTime) (records: RestRecord list) : Result<RestRecord list, string> =
        result {
            match records |> List.filter (isActive now) |> List.tryLast with
            | Some lastActive ->
                let! endedRest = lastActive |> tryCreateEnd now
                return records |> addToList endedRest
            | None -> return records
        }

    let toggleOfList (now: DateTime) (records: RestRecord list) : Result<RestRecord list, string> =
        match records |> isRestingOfList now  with
        | true -> records |> finishOfList now
        | false -> Ok(startOfList records)
