namespace AttendanceRecord.Domain.ValueObjects

open System
open AttendanceRecord.Domain.Errors

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

    let getDate (TimeDuration td) : DateTime = td.StartedAt.Date

    let tryCreate
        (startedAt: DateTime)
        (endedAt: DateTime option)
        : Result<TimeDuration, TimeDurationError> =
        match endedAt with
        | Some endDt when endDt < startedAt ->
            Error(EndedAtError "EndedAt is earlier than StartedAt")
        | Some endDt when endDt.Date <> startedAt.Date ->
            Error(EndedAtError "EndedAt must be on the same date as StartedAt")
        | _ -> Ok(hydrate startedAt endedAt)

    let createStart () : TimeDuration = hydrate DateTime.Now None

    let tryCreateEnd (TimeDuration td) : Result<TimeDuration, TimeDurationError> =
        match td.EndedAt with
        | Some _ -> Error(EndedAtError "Duration already ended")
        | None -> tryCreate td.StartedAt (Some DateTime.Now)

    let tryCreateRestart (TimeDuration td) : Result<TimeDuration, TimeDurationError> =
        match td.EndedAt with
        | None -> Error(EndedAtError "Duration is already active")
        | Some _ -> Ok(hydrate td.StartedAt None)
