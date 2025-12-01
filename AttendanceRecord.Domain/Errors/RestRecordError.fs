namespace AttendanceRecord.Domain.Errors

open System

type RestRecordError =
    | RestDurationError of Guid * TimeDurationError
    | RestGenericError of Guid * string

module RestRecordErrors =
    let chooseDuration (errors: RestRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | RestDurationError(id, TimeDurationError msg) -> [ (id, msg) ]
            | _ -> [])

    let chooseGeneric (errors: RestRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | RestGenericError(id, msg) -> [ (id, msg) ]
            | _ -> [])

    let chooseAll (errors: RestRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | RestDurationError(id, TimeDurationError msg) -> [ (id, msg) ]
            | RestGenericError(id, msg) -> [ (id, msg) ])
