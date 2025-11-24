namespace AttendanceRecord.Domain.Entities

open System
open FsToolkit.ErrorHandling

type ThemeMode =
    | SystemTheme // Follow system theme
    | LightTheme // Light theme
    | DarkTheme // Dark theme

type WorkEndAlarmConfig =
    { IsEnabled: bool
      BeforeEndDuration: TimeSpan
      SnoozeDuration: TimeSpan }

type RestStartAlarmConfig =
    { IsEnabled: bool
      BeforeStartDuration: TimeSpan
      SnoozeDuration: TimeSpan }

type AppConfig =
    { ThemeMode: ThemeMode
      StandardWorkTime: TimeSpan
      WorkEndAlarm: WorkEndAlarmConfig
      RestStartAlarm: RestStartAlarmConfig }

module WorkEndAlarmConfig =
    let initial: WorkEndAlarmConfig =
        { IsEnabled = true
          BeforeEndDuration = TimeSpan.FromMinutes 15.0
          SnoozeDuration = TimeSpan.FromMinutes 5.0 }

    let validate (config: WorkEndAlarmConfig) : Result<unit, string> =
        if config.BeforeEndDuration < TimeSpan.Zero then
            Error "Work end alarm 'before end' duration must be non-negative."
        else if config.SnoozeDuration < TimeSpan.Zero then
            Error "Work end alarm snooze duration must be non-negative."
        else
            Ok()

    let tryCreate
        (isEnabled: bool)
        (beforeEndDuration: TimeSpan)
        (snoozeDuration: TimeSpan)
        : Result<WorkEndAlarmConfig, string> =
        let config =
            { IsEnabled = isEnabled
              BeforeEndDuration = beforeEndDuration
              SnoozeDuration = snoozeDuration }

        result {
            do! validate config
            return config
        }

module RestStartAlarmConfig =
    let initial: RestStartAlarmConfig =
        { IsEnabled = true
          BeforeStartDuration = TimeSpan.FromMinutes 240.0
          SnoozeDuration = TimeSpan.FromMinutes 5.0 }

    let validate (config: RestStartAlarmConfig) : Result<unit, string> =
        if config.BeforeStartDuration < TimeSpan.Zero then
            Error "Rest start alarm 'before start' duration must be non-negative."
        else if config.SnoozeDuration < TimeSpan.Zero then
            Error "Rest start alarm snooze duration must be non-negative."
        else
            Ok()

    let tryCreate
        (isEnabled: bool)
        (beforeStartDuration: TimeSpan)
        (snoozeDuration: TimeSpan)
        : Result<RestStartAlarmConfig, string> =
        let config =
            { IsEnabled = isEnabled
              BeforeStartDuration = beforeStartDuration
              SnoozeDuration = snoozeDuration }

        result {
            do! validate config
            return config
        }

module AppConfig =
    let initial: AppConfig =
        { ThemeMode = SystemTheme
          StandardWorkTime = TimeSpan.FromHours 8.0
          WorkEndAlarm = WorkEndAlarmConfig.initial
          RestStartAlarm = RestStartAlarmConfig.initial }

    let validate (config: AppConfig) : Result<unit, string> =
        result {
            do! WorkEndAlarmConfig.validate config.WorkEndAlarm
            do! RestStartAlarmConfig.validate config.RestStartAlarm

            if config.StandardWorkTime <= TimeSpan.Zero then
                return! Error "Standard work time must be positive."
        }

    let tryCreate
        (themeMode: ThemeMode)
        (standardWorkTime: TimeSpan)
        (workEndAlarmConfig: WorkEndAlarmConfig)
        (restStartAlarmConfig: RestStartAlarmConfig)
        : Result<AppConfig, string> =
        let config =
            { ThemeMode = themeMode
              StandardWorkTime = standardWorkTime
              WorkEndAlarm = workEndAlarmConfig
              RestStartAlarm = restStartAlarmConfig }

        result {
            do! validate config
            return config
        }
