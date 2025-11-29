namespace AttendanceRecord.Domain.Entities

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Errors

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

type StandardWorkTime = StandardWorkTime of TimeSpan

type AppConfig =
    { ThemeMode: ThemeMode
      StandardWorkTime: StandardWorkTime
      WorkEndAlarm: WorkEndAlarmConfig
      RestStartAlarm: RestStartAlarmConfig }

module WorkEndAlarmConfig =
    let initial: WorkEndAlarmConfig =
        { IsEnabled = true
          BeforeEndDuration = TimeSpan.FromMinutes 15.0
          SnoozeDuration = TimeSpan.FromMinutes 5.0 }

    let tryCreate
        (isEnabled: bool)
        (beforeEndDuration: TimeSpan)
        (snoozeDuration: TimeSpan)
        : Validation<WorkEndAlarmConfig, AlarmConfigError> =
        validation {
            if beforeEndDuration < TimeSpan.Zero then
                return!
                    DurationError "Work end alarm 'before end' duration must be non-negative."
                    |> Error
            else if snoozeDuration < TimeSpan.Zero then
                return!
                    SnoozeDurationError "Work end alarm snooze duration must be non-negative."
                    |> Error
            else
                return
                    { IsEnabled = isEnabled
                      BeforeEndDuration = beforeEndDuration
                      SnoozeDuration = snoozeDuration }
        }

module RestStartAlarmConfig =
    let initial: RestStartAlarmConfig =
        { IsEnabled = true
          BeforeStartDuration = TimeSpan.FromMinutes 240.0
          SnoozeDuration = TimeSpan.FromMinutes 5.0 }

    let tryCreate
        (isEnabled: bool)
        (beforeStartDuration: TimeSpan)
        (snoozeDuration: TimeSpan)
        : Validation<RestStartAlarmConfig, AlarmConfigError> =
        validation {
            if beforeStartDuration < TimeSpan.Zero then
                return!
                    DurationError "Rest start alarm 'before start' duration must be non-negative."
                    |> Error
            else if snoozeDuration < TimeSpan.Zero then
                return!
                    SnoozeDurationError "Rest start alarm snooze duration must be non-negative."
                    |> Error
            else
                return
                    { IsEnabled = isEnabled
                      BeforeStartDuration = beforeStartDuration
                      SnoozeDuration = snoozeDuration }
        }

module StandardWorkTime =
    let value (StandardWorkTime ts) : TimeSpan = ts

    let tryCreate (ts: TimeSpan) : Result<StandardWorkTime, string> =
        if ts <= TimeSpan.Zero then
            Error "Standard work time must be positive."
        else
            Ok(StandardWorkTime ts)

    let hydrate (ts: TimeSpan) : StandardWorkTime = StandardWorkTime ts

module AppConfig =
    let initial: AppConfig =
        { ThemeMode = SystemTheme
          StandardWorkTime = StandardWorkTime(TimeSpan.FromHours 8.0)
          WorkEndAlarm = WorkEndAlarmConfig.initial
          RestStartAlarm = RestStartAlarmConfig.initial }

    let tryCreate
        (themeMode: ThemeMode)
        (standardWorkTime: TimeSpan)
        (workEndAlarmConfig: WorkEndAlarmConfig)
        (restStartAlarmConfig: RestStartAlarmConfig)
        : Validation<AppConfig, AppConfigError> =
        validation {
            let! standardWorkTimeVo =
                StandardWorkTime.tryCreate standardWorkTime
                |> Result.mapError (fun e -> StandardWorkTimeError e)

            return
                { ThemeMode = themeMode
                  StandardWorkTime = standardWorkTimeVo
                  WorkEndAlarm = workEndAlarmConfig
                  RestStartAlarm = restStartAlarmConfig }
        }
