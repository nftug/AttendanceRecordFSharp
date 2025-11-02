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

module AppConfig =
    let initial: AppConfig =
        { StandardWorkTime = TimeSpan.FromHours 8.0
          WorkEndAlarmConfig =
            { IsEnabled = true
              BeforeEndDuration = TimeSpan.FromMinutes 15.0
              SnoozeDuration = TimeSpan.FromMinutes 5.0 }
          RestStartAlarmConfig =
            { IsEnabled = true
              BeforeStartDuration = TimeSpan.FromMinutes 240.0
              SnoozeDuration = TimeSpan.FromMinutes 5.0 } }
