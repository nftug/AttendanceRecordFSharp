namespace AttendanceRecord.Application.Dtos.Requests

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Dtos.Responses

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

module WorkEndAlarmConfigSaveRequestDto =
    let empty: WorkEndAlarmConfigSaveRequestDto =
        { IsEnabled = true
          BeforeEndDurationMinutes = 15.0
          SnoozeDurationMinutes = 5.0 }

    let tryToDomain (dto: WorkEndAlarmConfigSaveRequestDto) : Result<WorkEndAlarmConfig, string> =
        WorkEndAlarmConfig.tryCreate
            dto.IsEnabled
            (TimeSpan.FromMinutes dto.BeforeEndDurationMinutes)
            (TimeSpan.FromMinutes dto.SnoozeDurationMinutes)

module RestStartAlarmConfigSaveRequestDto =
    let empty: RestStartAlarmConfigSaveRequestDto =
        { IsEnabled = true
          BeforeStartDurationMinutes = 240.0
          SnoozeDurationMinutes = 5.0 }

    let tryToDomain
        (dto: RestStartAlarmConfigSaveRequestDto)
        : Result<RestStartAlarmConfig, string> =
        RestStartAlarmConfig.tryCreate
            dto.IsEnabled
            (TimeSpan.FromMinutes dto.BeforeStartDurationMinutes)
            (TimeSpan.FromMinutes dto.SnoozeDurationMinutes)

module AppConfigSaveRequestDto =
    let empty: AppConfigSaveRequestDto =
        { StandardWorkTimeMinutes = 480.0
          WorkEndAlarmConfig = WorkEndAlarmConfigSaveRequestDto.empty
          RestStartAlarmConfig = RestStartAlarmConfigSaveRequestDto.empty }

    let tryToDomain (dto: AppConfigSaveRequestDto) : Result<AppConfig, string> =
        result {
            let! workEndAlarmConfig =
                WorkEndAlarmConfigSaveRequestDto.tryToDomain dto.WorkEndAlarmConfig

            let! restStartAlarmConfig =
                RestStartAlarmConfigSaveRequestDto.tryToDomain dto.RestStartAlarmConfig

            return
                { StandardWorkTime = TimeSpan.FromMinutes dto.StandardWorkTimeMinutes
                  WorkEndAlarmConfig = workEndAlarmConfig
                  RestStartAlarmConfig = restStartAlarmConfig }
        }

    let fromResponse (dto: AppConfigDto) : AppConfigSaveRequestDto =
        { AppConfigSaveRequestDto.StandardWorkTimeMinutes = dto.StandardWorkTimeMinutes
          WorkEndAlarmConfig =
            { IsEnabled = dto.WorkEndAlarmConfig.IsEnabled
              BeforeEndDurationMinutes = dto.WorkEndAlarmConfig.BeforeEndDurationMinutes
              SnoozeDurationMinutes = dto.WorkEndAlarmConfig.SnoozeDurationMinutes }
          RestStartAlarmConfig =
            { IsEnabled = dto.RestStartAlarmConfig.IsEnabled
              BeforeStartDurationMinutes = dto.RestStartAlarmConfig.BeforeStartDurationMinutes
              SnoozeDurationMinutes = dto.RestStartAlarmConfig.SnoozeDurationMinutes } }
