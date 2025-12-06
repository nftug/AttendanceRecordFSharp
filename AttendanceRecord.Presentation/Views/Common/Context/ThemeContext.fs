namespace AttendanceRecord.Presentation.Views.Common.Context

open R3
open Avalonia.Styling
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Enums

type ThemeContext =
   { ThemeMode: ReactiveProperty<ThemeModeEnum>
     LoadFromConfig: unit -> unit }

module ThemeContext =
   let create
      (appConfig: Observable<AppConfigDto>)
      (disposables: CompositeDisposable)
      : ThemeContext =
      let themeMode = R3.property ThemeModeEnum.SystemTheme |> R3.disposeWith disposables
      let appConfig = appConfig |> R3.readonly None |> R3.disposeWith disposables

      themeMode
      |> R3.subscribe (fun theme ->
         let variant =
            match theme with
            | ThemeModeEnum.SystemTheme -> ThemeVariant.Default
            | ThemeModeEnum.LightTheme -> ThemeVariant.Light
            | ThemeModeEnum.DarkTheme -> ThemeVariant.Dark
            | _ -> ThemeVariant.Default

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
