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
     RestStartAlarm: RestStartAlarmConfig
     WorkStatusFormat: string }

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
         if isEnabled && beforeEndDuration < TimeSpan.Zero then
            return! DurationError "アラームの設定は0分以上にしてください。" |> Error
         else if isEnabled && snoozeDuration <= TimeSpan.Zero then
            return! SnoozeDurationError "アラームのスヌーズ時間は0分より大きい値にしてください。" |> Error
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
         if isEnabled && beforeStartDuration < TimeSpan.Zero then
            return! DurationError "アラームの設定は0分以上にしてください。" |> Error
         else if isEnabled && snoozeDuration <= TimeSpan.Zero then
            return! SnoozeDurationError "アラームのスヌーズ時間は0分より大きい値にしてください。" |> Error
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
         Error "標準勤務時間は0分より大きい値にしてください。"
      else
         Ok(StandardWorkTime ts)

   let hydrate (ts: TimeSpan) : StandardWorkTime = StandardWorkTime ts

module AppConfig =
   let initial: AppConfig =
      { ThemeMode = SystemTheme
        StandardWorkTime = StandardWorkTime(TimeSpan.FromHours 8.0)
        WorkEndAlarm = WorkEndAlarmConfig.initial
        RestStartAlarm = RestStartAlarmConfig.initial
        WorkStatusFormat =
         """- 勤務時間: {work_hh}時間{work_mm}分
- 休憩時間: {rest_hh}時間{rest_mm}分
- 今日の残業時間: {over_hh}時間{over_mm}分
- 今月の残業時間: {over_monthly_hh}時間{over_monthly_mm}分""" }

   let tryCreate
      (themeMode: ThemeMode)
      (standardWorkTime: TimeSpan)
      (workEndAlarmConfig: WorkEndAlarmConfig)
      (restStartAlarmConfig: RestStartAlarmConfig)
      (workStatusFormat: string)
      : Validation<AppConfig, AppConfigError> =
      validation {
         let! standardWorkTimeVo =
            StandardWorkTime.tryCreate standardWorkTime
            |> Result.mapError StandardWorkTimeError

         let standardWorkTime = StandardWorkTime.value standardWorkTimeVo

         if workEndAlarmConfig.BeforeEndDuration > standardWorkTime then
            return! WorkEndAlarmError(DurationError "アラームの設定は標準勤務時間を超えないようにしてください。") |> Error
         else if restStartAlarmConfig.BeforeStartDuration > standardWorkTime then
            return! RestStartAlarmError(DurationError "アラームの設定は標準勤務時間を超えないようにしてください。") |> Error
         else
            return
               { ThemeMode = themeMode
                 StandardWorkTime = standardWorkTimeVo
                 WorkEndAlarm = workEndAlarmConfig
                 RestStartAlarm = restStartAlarmConfig
                 WorkStatusFormat = workStatusFormat }
      }
