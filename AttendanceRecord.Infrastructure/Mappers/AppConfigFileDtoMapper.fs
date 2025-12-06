namespace AttendanceRecord.Infrastructure.Mappers

open System
open AttendanceRecord.Persistence.Dtos
open AttendanceRecord.Domain.Entities

module AppConfigFileDtoMapper =
   let fromDomain (config: AppConfig) : AppConfigFileDto =
      AppConfigFileDto(
         match config.ThemeMode with
         | SystemTheme -> "System"
         | LightTheme -> "Light"
         | DarkTheme -> "Dark"
         , float (config.StandardWorkTime |> StandardWorkTime.value |> _.TotalMinutes)
         , WorkEndAlarmConfigFileDto(
            config.WorkEndAlarm.IsEnabled,
            float config.WorkEndAlarm.BeforeEndDuration.TotalMinutes,
            float config.WorkEndAlarm.SnoozeDuration.TotalMinutes
         )
         , RestStartAlarmConfigFileDto(
            config.RestStartAlarm.IsEnabled,
            float config.RestStartAlarm.BeforeStartDuration.TotalMinutes,
            float config.RestStartAlarm.SnoozeDuration.TotalMinutes
         )
         , config.WorkStatusFormat
      )

   let toDomain (dto: AppConfigFileDto) : AppConfig =
      { ThemeMode =
         match dto.ThemeMode with
         | "System" -> SystemTheme
         | "Light" -> LightTheme
         | "Dark" -> DarkTheme
         | _ -> SystemTheme // Default to System if unrecognized
        StandardWorkTime =
         TimeSpan.FromMinutes(float dto.StandardWorkMinutes) |> StandardWorkTime.hydrate
        WorkEndAlarm =
         { IsEnabled = dto.WorkEndAlarm.IsEnabled
           BeforeEndDuration = TimeSpan.FromMinutes(float dto.WorkEndAlarm.BeforeEndMinutes)
           SnoozeDuration = TimeSpan.FromMinutes(float dto.WorkEndAlarm.SnoozeMinutes) }
        RestStartAlarm =
         { IsEnabled = dto.RestStartAlarm.IsEnabled
           BeforeStartDuration = TimeSpan.FromMinutes(float dto.RestStartAlarm.BeforeStartMinutes)
           SnoozeDuration = TimeSpan.FromMinutes(float dto.RestStartAlarm.SnoozeMinutes) }
        WorkStatusFormat =
         dto.WorkStatusFormat
         |> Option.ofObj
         |> Option.defaultValue AppConfig.initial.WorkStatusFormat }
