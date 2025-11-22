namespace AttendanceRecord.Application.Dtos.Requests

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities

type WorkEndAlarmConfigSaveRequestDto =
    { IsEnabled: bool
      BeforeEndDurationMinutes: float
      SnoozeDurationMinutes: float }

type RestStartAlarmConfigSaveRequestDto =
    { IsEnabled: bool
      BeforeStartDurationMinutes: float
      SnoozeDurationMinutes: float }

type AppConfigSaveRequestDto =
    { StandardWorkTimeMinutes: float
      WorkEndAlarmConfig: WorkEndAlarmConfigSaveRequestDto
      RestStartAlarmConfig: RestStartAlarmConfigSaveRequestDto }

module AppConfigSaveRequestDto =
    let tryToDomain (dto: AppConfigSaveRequestDto) : Result<AppConfig, string> =
        result {
            let! workEndAlarmConfig =
                WorkEndAlarmConfig.tryCreate
                    dto.WorkEndAlarmConfig.IsEnabled
                    (TimeSpan.FromMinutes dto.WorkEndAlarmConfig.BeforeEndDurationMinutes)
                    (TimeSpan.FromMinutes dto.WorkEndAlarmConfig.SnoozeDurationMinutes)

            let! restStartAlarmConfig =
                RestStartAlarmConfig.tryCreate
                    dto.RestStartAlarmConfig.IsEnabled
                    (TimeSpan.FromMinutes dto.RestStartAlarmConfig.BeforeStartDurationMinutes)
                    (TimeSpan.FromMinutes dto.RestStartAlarmConfig.SnoozeDurationMinutes)

            return
                { StandardWorkTime = TimeSpan.FromMinutes dto.StandardWorkTimeMinutes
                  WorkEndAlarmConfig = workEndAlarmConfig
                  RestStartAlarmConfig = restStartAlarmConfig }
        }
