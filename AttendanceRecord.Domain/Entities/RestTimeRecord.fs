namespace AttendanceRecord.Domain.Entities

open System
open AttendanceRecord.Domain.ValueObjects

type RestTimeRecord = { Id: Guid; Duration: TimeDuration }

module RestTimeRecord =
    // Factory methods
    let hydrate (id: Guid) (duration: TimeDuration) : RestTimeRecord = { Id = id; Duration = duration }

    let create (duration: TimeDuration) : RestTimeRecord =
        { Id = Guid.NewGuid()
          Duration = duration }

    let createStart () : RestTimeRecord = create (TimeDuration.createStart ())

    let createEnd (record: RestTimeRecord) : Result<RestTimeRecord, string> =
        match record.Duration |> TimeDuration.isActive with
        | true ->
            TimeDuration.createEnd record.Duration
            |> Result.map (fun duration -> { record with Duration = duration })
        | false -> Error "Rest time is not active"

    // Status getters
    let getDuration (record: RestTimeRecord) : TimeSpan =
        record.Duration |> TimeDuration.getDuration

    let isActive (record: RestTimeRecord) : bool =
        record.Duration |> TimeDuration.isActive

    let getStartedAt (record: RestTimeRecord) : DateTime =
        record.Duration |> TimeDuration.getStartedAt

    let getEndedAt (record: RestTimeRecord) : DateTime option =
        record.Duration |> TimeDuration.getEndedAt

    // List operations
    let getSortedList (records: RestTimeRecord list) : RestTimeRecord list =
        records |> List.sortBy getStartedAt

    let isRestingOfList (records: RestTimeRecord list) : bool =
        records |> List.tryLast |> Option.map isActive |> Option.defaultValue false

    let getDurationOfList (records: RestTimeRecord list) : TimeSpan =
        records
        |> List.sumBy (getDuration >> _.Ticks)
        |> TimeSpan.FromTicks

    let startOfList (records: RestTimeRecord list) : RestTimeRecord list = records @ [ createStart () ]

    let finishOfList (records: RestTimeRecord list) : RestTimeRecord list =
        match records |> List.filter isActive |> List.tryLast with
        | Some last ->
            createEnd last
            |> function
                | Ok endedRest -> (records |> List.take (records.Length - 1)) @ [ endedRest ]
                | Error _ -> records
        | _ -> records

    let toggleOfList (records: RestTimeRecord list) : RestTimeRecord list =
        match isRestingOfList records with
        | true -> finishOfList records
        | false -> startOfList records
