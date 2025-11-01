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

    let isActive (TimeDuration td) : bool =
        match td.StartedAt, td.EndedAt with
        | start, None when start.Date = DateTime.Now.Date -> true
        | _ -> false

    let getDuration (TimeDuration td) : TimeSpan =
        match td.StartedAt, td.EndedAt with
        | start, None when start.Date = DateTime.Now.Date -> DateTime.Now - start
        | start, None -> start.Date.AddDays 1 - start
        | start, Some endDt -> endDt - start

    let getStartedAt (TimeDuration td) : DateTime = td.StartedAt

    let getEndedAt (TimeDuration td) : DateTime option = td.EndedAt

    let create (startedAt: DateTime) (endedAt: DateTime option) : Result<TimeDuration, string> =
        if endedAt.IsSome && endedAt.Value < startedAt then
            Error "Invalid time duration"
        else
            Ok(hydrate startedAt endedAt)

    let createStart () : TimeDuration = hydrate DateTime.Now None

    let createEnd (TimeDuration td) : Result<TimeDuration, string> =
        match td.EndedAt with
        | Some _ -> Error "Duration already ended"
        | None -> create td.StartedAt (Some DateTime.Now)

    let createRestart (TimeDuration _) : Result<TimeDuration, string> = create DateTime.Now None
