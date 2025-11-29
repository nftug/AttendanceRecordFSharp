namespace AttendanceRecord.Domain.Errors

open System

type WorkRecordError =
    | WorkDurationError of TimeDurationError
    | WorkRestsError of RestRecordError list
    | WorkVariantError of string

module WorkRecordErrors =
    let duration (error: TimeDurationError) : WorkRecordError list = [ WorkDurationError error ]

    let rest (error: RestRecordError) : WorkRecordError list = [ WorkRestsError [ error ] ]

    let restList (errors: RestRecordError list) : WorkRecordError list = [ WorkRestsError errors ]

    let variant (error: string) : WorkRecordError list = [ WorkVariantError error ]

    let chooseDuration (errors: WorkRecordError list) : string list =
        errors
        |> List.collect (function
            | WorkDurationError(TimeDurationError msg) -> [ msg ]
            | _ -> [])

    let chooseRestsDuration (restId: Guid) (errors: WorkRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | WorkRestsError restErrors -> RestRecordErrors.chooseDuration restErrors
            | _ -> [])

    let chooseRestsVariants (errors: WorkRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | WorkRestsError restErrors -> RestRecordErrors.chooseVariants restErrors
            | _ -> [])

    let chooseRestsAll (errors: WorkRecordError list) : (Guid * string) list =
        errors
        |> List.collect (function
            | WorkRestsError restErrors -> RestRecordErrors.chooseAll restErrors
            | _ -> [])

    let chooseVariants (errors: WorkRecordError list) : string list =
        errors
        |> List.collect (function
            | WorkVariantError msg -> [ msg ]
            | _ -> [])

    let chooseDurationOrVariants (errors: WorkRecordError list) : string list =
        errors
        |> List.collect (function
            | WorkDurationError(TimeDurationError msg) -> [ msg ]
            | WorkVariantError msg -> [ msg ]
            | _ -> [])
