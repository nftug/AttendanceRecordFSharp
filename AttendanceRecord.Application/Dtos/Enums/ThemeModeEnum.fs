namespace AttendanceRecord.Application.Dtos.Enums

[<RequireQualifiedAccess>]
type ThemeModeEnum =
    | SystemTheme = 0
    | LightTheme = 1
    | DarkTheme = 2

module ThemeModeEnum =
    open AttendanceRecord.Domain.Entities

    let all =
        [ ThemeModeEnum.SystemTheme; ThemeModeEnum.LightTheme; ThemeModeEnum.DarkTheme ]

    let fromDomain (mode: ThemeMode) : ThemeModeEnum =
        match mode with
        | SystemTheme -> ThemeModeEnum.SystemTheme
        | LightTheme -> ThemeModeEnum.LightTheme
        | DarkTheme -> ThemeModeEnum.DarkTheme

    let toDomain (mode: ThemeModeEnum) : ThemeMode =
        match mode with
        | ThemeModeEnum.SystemTheme -> SystemTheme
        | ThemeModeEnum.LightTheme -> LightTheme
        | ThemeModeEnum.DarkTheme -> DarkTheme
        | _ -> SystemTheme

    let toDisplayName (mode: ThemeModeEnum) : string =
        match mode with
        | ThemeModeEnum.SystemTheme -> "システム"
        | ThemeModeEnum.LightTheme -> "ライト"
        | ThemeModeEnum.DarkTheme -> "ダーク"
        | _ -> "システム"
