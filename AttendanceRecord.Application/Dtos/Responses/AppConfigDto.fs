namespace AttendanceRecord.Application.Dtos.Responses

open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Dtos.Enums

type WorkEndAlarmConfigDto =
    { IsEnabled: bool
      BeforeEndMinutes: float
      SnoozeMinutes: float }

type RestStartAlarmConfigDto =
    { IsEnabled: bool
      BeforeStartMinutes: float
      SnoozeMinutes: float }

type AppConfigDto =
    { ThemeMode: ThemeModeEnum
      StandardWorkTimeMinutes: float
      WorkEndAlarm: WorkEndAlarmConfigDto
      RestStartAlarm: RestStartAlarmConfigDto }

module AppConfigDto =
    let fromDomain (config: AppConfig) : AppConfigDto =
        { ThemeMode = config.ThemeMode |> ThemeModeEnum.fromDomain
          StandardWorkTimeMinutes =
            config.StandardWorkTime |> StandardWorkTime.value |> _.TotalMinutes
          WorkEndAlarm =
            { IsEnabled = config.WorkEndAlarm.IsEnabled
              BeforeEndMinutes = config.WorkEndAlarm.BeforeEndDuration.TotalMinutes
              SnoozeMinutes = config.WorkEndAlarm.SnoozeDuration.TotalMinutes }
          RestStartAlarm =
            { IsEnabled = config.RestStartAlarm.IsEnabled
              BeforeStartMinutes = config.RestStartAlarm.BeforeStartDuration.TotalMinutes
              SnoozeMinutes = config.RestStartAlarm.SnoozeDuration.TotalMinutes } }
