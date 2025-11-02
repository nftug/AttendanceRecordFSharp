namespace AttendanceRecord.Domain.ValueObjects

open System

type private TimeDurationRecord =
    { StartedAt: DateTime
      EndedAt: DateTime option }

type TimeDuration = private TimeDuration of TimeDurationRecord

module TimeDuration =
    let hydrate (startedAt: DateTime) (endedAt: DateTime option) : TimeDuration =
        TimeDuration
            { StartedAt = startedAt
              EndedAt = endedAt }

    let isActive (now: DateTime) (TimeDuration td) : bool =
        match td.StartedAt, td.EndedAt with
        | start, None when start.Date = now.Date -> true
        | _ -> false

    let getDuration (now: DateTime) (TimeDuration td) : TimeSpan =
        match td.StartedAt, td.EndedAt with
        | start, None when start.Date = now.Date -> now - start
        | start, None -> start.Date.AddDays 1 - start
        | start, Some endDt -> endDt - start

    let getStartedAt (TimeDuration td) : DateTime = td.StartedAt

    let getEndedAt (TimeDuration td) : DateTime option = td.EndedAt

    let create (startedAt: DateTime) (endedAt: DateTime option) : Result<TimeDuration, string> =
        if endedAt.IsSome && endedAt.Value < startedAt then
            Error "Invalid time duration"
        else if endedAt.IsSome && endedAt.Value.Date <> startedAt.Date then
            Error "EndedAt must be on the same date as StartedAt"
        else
            Ok(hydrate startedAt endedAt)

    let createStart () : TimeDuration = hydrate DateTime.Now None

    let createEnd (TimeDuration td) : Result<TimeDuration, string> =
        match td.EndedAt with
        | Some _ -> Error "Duration already ended"
        | None -> create td.StartedAt (Some DateTime.Now)

    let createRestart (TimeDuration td) : Result<TimeDuration, string> =
        match td.EndedAt with
        | None -> Error "Duration is already active"
        | Some _ -> Ok(hydrate td.StartedAt None)
