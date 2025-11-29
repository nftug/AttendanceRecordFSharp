namespace AttendanceRecord.Domain.Errors

open System

type RestRecordError =
    | RestDurationError of Guid * TimeDurationError
    | RestVariantError of Guid * string

module RestRecordErrors =
    let chooseStartedAt (errors: RestRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | RestDurationError(id, StartedAtError msg) -> [ (id, msg) ]
            | _ -> [])

    let chooseEndedAt (errors: RestRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | RestDurationError(id, EndedAtError msg) -> [ (id, msg) ]
            | _ -> [])
