namespace AttendanceRecord.Presentation.Views.Common

open R3
open Avalonia.Styling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Responses

type ThemeContext =
    { ThemeMode: ReactiveProperty<ThemeMode>
      LoadFromConfig: unit -> unit }

module ThemeContext =
    let create
        (appConfig: Observable<AppConfigDto>)
        (disposables: CompositeDisposable)
        : ThemeContext =
        let themeMode = R3.property SystemTheme |> R3.disposeWith disposables
        let appConfig = appConfig |> R3.readonly None |> R3.disposeWith disposables

        themeMode
        |> R3.subscribe (fun theme ->
            let variant =
                match theme with
                | SystemTheme -> ThemeVariant.Default
                | LightTheme -> ThemeVariant.Light
                | DarkTheme -> ThemeVariant.Dark

            Avalonia.Application.Current.RequestedThemeVariant <- variant
            themeMode.Value <- theme)
        |> disposables.Add

        appConfig
        |> R3.map _.ThemeMode
        |> R3.subscribe (fun theme -> themeMode.Value <- theme)
        |> disposables.Add

        let loadFromConfig () =
            let config = appConfig.CurrentValue
            themeMode.Value <- config.ThemeMode

        { ThemeMode = themeMode
          LoadFromConfig = loadFromConfig }
