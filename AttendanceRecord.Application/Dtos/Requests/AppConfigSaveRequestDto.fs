namespace AttendanceRecord.Application.Dtos.Requests

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Enums

type WorkEndAlarmConfigSaveRequestDto =
    { IsEnabled: bool
      BeforeEndMinutes: float
      SnoozeMinutes: float }

type RestStartAlarmConfigSaveRequestDto =
    { IsEnabled: bool
      BeforeStartMinutes: float
      SnoozeMinutes: float }

type AppConfigSaveRequestDto =
    { ThemeMode: ThemeModeEnum
      StandardWorkTimeMinutes: float
      WorkEndAlarm: WorkEndAlarmConfigSaveRequestDto
      RestStartAlarm: RestStartAlarmConfigSaveRequestDto
      WorkStatusFormat: string }

module WorkEndAlarmConfigSaveRequestDto =
    let empty: WorkEndAlarmConfigSaveRequestDto =
        { IsEnabled = true
          BeforeEndMinutes = 15.0
          SnoozeMinutes = 5.0 }

    let tryToDomain
        (dto: WorkEndAlarmConfigSaveRequestDto)
        : Validation<WorkEndAlarmConfig, AlarmConfigError> =
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
        : Validation<RestStartAlarmConfig, AlarmConfigError> =
        RestStartAlarmConfig.tryCreate
            dto.IsEnabled
            (TimeSpan.FromMinutes dto.BeforeStartMinutes)
            (TimeSpan.FromMinutes dto.SnoozeMinutes)

module AppConfigSaveRequestDto =
    let empty: AppConfigSaveRequestDto =
        { ThemeMode = ThemeModeEnum.SystemTheme
          StandardWorkTimeMinutes = 480.0
          WorkEndAlarm = WorkEndAlarmConfigSaveRequestDto.empty
          RestStartAlarm = RestStartAlarmConfigSaveRequestDto.empty
          WorkStatusFormat = AppConfig.initial.WorkStatusFormat }

    let tryToDomain (dto: AppConfigSaveRequestDto) : Validation<AppConfig, AppConfigError> =
        validation {
            let! workEndAlarmConfig =
                WorkEndAlarmConfigSaveRequestDto.tryToDomain dto.WorkEndAlarm
                |> Result.mapError (List.map WorkEndAlarmError)

            let! restStartAlarmConfig =
                RestStartAlarmConfigSaveRequestDto.tryToDomain dto.RestStartAlarm
                |> Result.mapError (List.map RestStartAlarmError)

            return!
                AppConfig.tryCreate
                    (dto.ThemeMode |> ThemeModeEnum.toDomain)
                    (TimeSpan.FromMinutes dto.StandardWorkTimeMinutes)
                    workEndAlarmConfig
                    restStartAlarmConfig
                    dto.WorkStatusFormat
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
              SnoozeMinutes = dto.RestStartAlarm.SnoozeMinutes }
          WorkStatusFormat = dto.WorkStatusFormat }
