namespace AttendanceRecord.Application.Dtos.Requests

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Dtos.Responses

type WorkEndAlarmConfigSaveRequestDto =
    { IsEnabled: bool
      BeforeEndMinutes: float
      SnoozeMinutes: float }

type RestStartAlarmConfigSaveRequestDto =
    { IsEnabled: bool
      BeforeStartMinutes: float
      SnoozeMinutes: float }

type AppConfigSaveRequestDto =
    { ThemeMode: ThemeMode
      StandardWorkTimeMinutes: float
      WorkEndAlarm: WorkEndAlarmConfigSaveRequestDto
      RestStartAlarm: RestStartAlarmConfigSaveRequestDto }

module WorkEndAlarmConfigSaveRequestDto =
    let empty: WorkEndAlarmConfigSaveRequestDto =
        { IsEnabled = true
          BeforeEndMinutes = 15.0
          SnoozeMinutes = 5.0 }

    let tryToDomain (dto: WorkEndAlarmConfigSaveRequestDto) : Result<WorkEndAlarmConfig, string> =
        WorkEndAlarmConfig.tryCreate
            dto.IsEnabled
            (TimeSpan.FromMinutes dto.BeforeEndMinutes)
            (TimeSpan.FromMinutes dto.SnoozeMinutes)

module RestStartAlarmConfigSaveRequestDto =
    let empty: RestStartAlarmConfigSaveRequestDto =
        { IsEnabled = true
          BeforeStartMinutes = 240.0
          SnoozeMinutes = 5.0 }

    let tryToDomain
        (dto: RestStartAlarmConfigSaveRequestDto)
        : Result<RestStartAlarmConfig, string> =
        RestStartAlarmConfig.tryCreate
            dto.IsEnabled
            (TimeSpan.FromMinutes dto.BeforeStartMinutes)
            (TimeSpan.FromMinutes dto.SnoozeMinutes)

module AppConfigSaveRequestDto =
    let empty: AppConfigSaveRequestDto =
        { ThemeMode = SystemTheme
          StandardWorkTimeMinutes = 480.0
          WorkEndAlarm = WorkEndAlarmConfigSaveRequestDto.empty
          RestStartAlarm = RestStartAlarmConfigSaveRequestDto.empty }

    let tryToDomain (dto: AppConfigSaveRequestDto) : Result<AppConfig, string> =
        result {
            let! workEndAlarmConfig = WorkEndAlarmConfigSaveRequestDto.tryToDomain dto.WorkEndAlarm

            let! restStartAlarmConfig =
                RestStartAlarmConfigSaveRequestDto.tryToDomain dto.RestStartAlarm

            return
                { ThemeMode = dto.ThemeMode
                  StandardWorkTime = TimeSpan.FromMinutes dto.StandardWorkTimeMinutes
                  WorkEndAlarm = workEndAlarmConfig
                  RestStartAlarm = restStartAlarmConfig }
        }

    let fromResponse (dto: AppConfigDto) : AppConfigSaveRequestDto =
        { AppConfigSaveRequestDto.ThemeMode = dto.ThemeMode
          AppConfigSaveRequestDto.StandardWorkTimeMinutes = dto.StandardWorkTimeMinutes
          WorkEndAlarm =
            { IsEnabled = dto.WorkEndAlarm.IsEnabled
              BeforeEndMinutes = dto.WorkEndAlarm.BeforeEndMinutes
              SnoozeMinutes = dto.WorkEndAlarm.SnoozeMinutes }
          RestStartAlarm =
            { IsEnabled = dto.RestStartAlarm.IsEnabled
              BeforeStartMinutes = dto.RestStartAlarm.BeforeStartMinutes
              SnoozeMinutes = dto.RestStartAlarm.SnoozeMinutes } }
