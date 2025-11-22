namespace AttendanceRecord.Application.Dtos.Responses

open AttendanceRecord.Domain.Entities

type WorkEndAlarmConfigDto =
    { IsEnabled: bool
      BeforeEndDurationMinutes: float
      SnoozeDurationMinutes: float }

type RestStartAlarmConfigDto =
    { IsEnabled: bool
      BeforeStartDurationMinutes: float
      SnoozeDurationMinutes: float }

type AppConfigDto =
    { ThemeMode: ThemeMode
      StandardWorkTimeMinutes: float
      WorkEndAlarmConfig: WorkEndAlarmConfigDto
      RestStartAlarmConfig: RestStartAlarmConfigDto }

module AppConfigDto =
    let fromDomain (config: AppConfig) : AppConfigDto =
        { ThemeMode = config.ThemeMode
          StandardWorkTimeMinutes = config.StandardWorkTime.TotalMinutes
          WorkEndAlarmConfig =
            { IsEnabled = config.WorkEndAlarmConfig.IsEnabled
              BeforeEndDurationMinutes = config.WorkEndAlarmConfig.BeforeEndDuration.TotalMinutes
              SnoozeDurationMinutes = config.WorkEndAlarmConfig.SnoozeDuration.TotalMinutes }
          RestStartAlarmConfig =
            { IsEnabled = config.RestStartAlarmConfig.IsEnabled
              BeforeStartDurationMinutes =
                config.RestStartAlarmConfig.BeforeStartDuration.TotalMinutes
              SnoozeDurationMinutes = config.RestStartAlarmConfig.SnoozeDuration.TotalMinutes } }
