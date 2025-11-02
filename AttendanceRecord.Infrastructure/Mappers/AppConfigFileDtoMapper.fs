namespace AttendanceRecord.Infrastructure.Mappers

open System
open AttendanceRecord.Persistence.Dtos
open AttendanceRecord.Domain.Entities

module AppConfigFileDtoMapper =
    let fromDomain (config: AppConfig) : AppConfigFileDto =
        AppConfigFileDto(
            float config.StandardWorkTime.TotalMinutes,
            WorkEndAlarmConfigFileDto(
                config.WorkEndAlarmConfig.IsEnabled,
                float config.WorkEndAlarmConfig.BeforeEndDuration.TotalMinutes,
                float config.WorkEndAlarmConfig.SnoozeDuration.TotalMinutes
            ),
            RestStartAlarmConfigFileDto(
                config.RestStartAlarmConfig.IsEnabled,
                float config.RestStartAlarmConfig.BeforeStartDuration.TotalMinutes,
                float config.RestStartAlarmConfig.SnoozeDuration.TotalMinutes
            )
        )

    let toDomain (dto: AppConfigFileDto) : AppConfig =
        { StandardWorkTime = TimeSpan.FromMinutes(float dto.StandardWorkMinutes)
          WorkEndAlarmConfig =
            { IsEnabled = dto.WorkEndAlarmConfig.IsEnabled
              BeforeEndDuration = TimeSpan.FromMinutes(float dto.WorkEndAlarmConfig.BeforeEndMinutes)
              SnoozeDuration = TimeSpan.FromMinutes(float dto.WorkEndAlarmConfig.SnoozeMinutes) }
          RestStartAlarmConfig =
            { IsEnabled = dto.RestStartAlarmConfig.IsEnabled
              BeforeStartDuration = TimeSpan.FromMinutes(float dto.RestStartAlarmConfig.BeforeStartMinutes)
              SnoozeDuration = TimeSpan.FromMinutes(float dto.RestStartAlarmConfig.SnoozeMinutes) } }
