namespace AttendanceRecord.Domain.Errors

open System

type RestRecordError =
    | RestDurationError of Guid * TimeDurationError
    | RestVariantError of Guid * string

module RestRecordErrors =
    let chooseDuration (errors: RestRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | RestDurationError(id, TimeDurationError msg) -> [ (id, msg) ]
            | _ -> [])

    let chooseVariants (errors: RestRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | RestVariantError(id, msg) -> [ (id, msg) ]
            | _ -> [])

    let chooseAll (errors: RestRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | RestDurationError(id, TimeDurationError msg) -> [ (id, msg) ]
            | RestVariantError(id, msg) -> [ (id, msg) ])
