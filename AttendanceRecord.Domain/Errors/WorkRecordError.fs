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

    let chooseStartedAt (errors: WorkRecordError list) : string list =
        errors
        |> List.collect (function
            | WorkDurationError(StartedAtError msg) -> [ msg ]
            | _ -> [])

    let chooseEndedAt (errors: WorkRecordError list) : string list =
        errors
        |> List.collect (function
            | WorkDurationError(EndedAtError msg) -> [ msg ]
            | _ -> [])

    let chooseRestStartedAt (id: Guid) (errors: WorkRecordError list) : string list =
        errors
        |> List.collect (function
            | WorkRestsError restErrors ->
                RestRecordErrors.chooseStartedAt restErrors
                |> List.filter (fun (errId, _) -> errId = id)
                |> List.map snd
            | _ -> [])

    let chooseRestEndedAt (id: Guid) (errors: WorkRecordError list) : string list =
        errors
        |> List.collect (function
            | WorkRestsError restErrors ->
                RestRecordErrors.chooseEndedAt restErrors
                |> List.filter (fun (errId, _) -> errId = id)
                |> List.map snd
            | _ -> [])

    let chooseVariants (errors: WorkRecordError list) : string list =
        errors
        |> List.collect (function
            | WorkVariantError msg -> [ msg ]
            | _ -> [])
