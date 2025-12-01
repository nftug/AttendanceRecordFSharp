namespace AttendanceRecord.Domain.Errors

type AlarmConfigError =
    | DurationError of string
    | SnoozeDurationError of string

type AppConfigError =
    | StandardWorkTimeError of string
    | WorkEndAlarmError of AlarmConfigError
    | RestStartAlarmError of AlarmConfigError
    | GenericError of string

module AppConfigErrors =
    let generic (error: string) : AppConfigError list = [ GenericError error ]

    let chooseStandardWorkTime (errors: AppConfigError list) : string list =
        errors
        |> List.collect (function
            | StandardWorkTimeError msg -> [ msg ]
            | _ -> [])

    let chooseWorkEndAlarmDuration (errors: AppConfigError list) : string list =
        errors
        |> List.collect (function
            | WorkEndAlarmError(DurationError msg) -> [ msg ]
            | _ -> [])

    let chooseWorkEndAlarmSnoozeDuration (errors: AppConfigError list) : string list =
        errors
        |> List.collect (function
            | WorkEndAlarmError(SnoozeDurationError msg) -> [ msg ]
            | _ -> [])

    let chooseRestStartAlarmDuration (errors: AppConfigError list) : string list =
        errors
        |> List.collect (function
            | RestStartAlarmError(DurationError msg) -> [ msg ]
            | _ -> [])

    let chooseRestStartAlarmSnoozeDuration (errors: AppConfigError list) : string list =
        errors
        |> List.collect (function
            | RestStartAlarmError(SnoozeDurationError msg) -> [ msg ]
            | _ -> [])

    let chooseVariants (errors: AppConfigError list) : string list =
        errors
        |> List.collect (function
            | GenericError msg -> [ msg ]
            | _ -> [])
