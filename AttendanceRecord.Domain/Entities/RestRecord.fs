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

    let createEnd (record: RestRecord) : Result<RestRecord, string> =
        match record.Duration |> TimeDuration.isActive with
        | true ->
            TimeDuration.createEnd record.Duration
            |> Result.map (fun duration -> { record with Duration = duration })
        | false -> Error "Rest time is not active"

    // Status getters
    let getDuration (record: RestRecord) : TimeSpan =
        record.Duration |> TimeDuration.getDuration

    let isActive (record: RestRecord) : bool =
        record.Duration |> TimeDuration.isActive

    let getStartedAt (record: RestRecord) : DateTime =
        record.Duration |> TimeDuration.getStartedAt

    let getEndedAt (record: RestRecord) : DateTime option =
        record.Duration |> TimeDuration.getEndedAt

    // List operations
    let getSortedList (records: RestRecord list) : RestRecord list =
        records |> List.sortBy getStartedAt

    let isRestingOfList (records: RestRecord list) : bool =
        records |> List.filter isActive |> List.tryLast |> Option.map (fun _ -> true) |> Option.defaultValue false

    let getDurationOfList (records: RestRecord list) : TimeSpan =
        records
        |> List.sumBy (getDuration >> _.Ticks)
        |> TimeSpan.FromTicks

    let addToList (record: RestRecord) (records: RestRecord list) : RestRecord list =
        records
        |> List.filter (fun r -> r.Id <> record.Id)
        |> List.append [ record ]
        |> getSortedList

    let startOfList (records: RestRecord list) : RestRecord list = records |> addToList (createStart ())

    let finishOfList (records: RestRecord list) : Result<RestRecord list, string> =
        result {
            match records |> List.filter isActive |> List.tryLast with
            | Some lastActive ->
                let! endedRest = createEnd lastActive
                return records |> addToList endedRest
            | None -> return records
        }

    let toggleOfList (records: RestRecord list) : Result<RestRecord list, string> =
        match isRestingOfList records with
        | true -> finishOfList records
        | false -> Ok(startOfList records)
