namespace AttendanceRecord.Domain.Entities

open System

type WorkEndAlarmConfig =
    { IsEnabled: bool
      BeforeEndDuration: TimeSpan
      SnoozeDuration: TimeSpan }

type RestStartAlarmConfig =
    { IsEnabled: bool
      BeforeStartDuration: TimeSpan
      SnoozeDuration: TimeSpan }

type AppConfig =
    { StandardWorkTime: TimeSpan
      WorkEndAlarmConfig: WorkEndAlarmConfig
      RestStartAlarmConfig: RestStartAlarmConfig }
