namespace AttendanceRecord.Application.UseCases.AppConfig

open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Services

type SaveAppConfig =
    { Handle: AppConfigSaveRequestDto -> CancellationToken -> TaskResult<unit, AppConfigError list> }

module SaveAppConfig =
    let private handle
        (repository: AppConfigRepository)
        (appConfigStore: AppConfigStore)
        (request: AppConfigSaveRequestDto)
        (ct: CancellationToken)
        =
        taskResult {
            let! config = AppConfigSaveRequestDto.tryToDomain request
            do! repository.SaveConfig config ct |> TaskResult.mapError AppConfigErrors.generic
            appConfigStore.Set config
        }

    let create (repository: AppConfigRepository) (appConfigStore: AppConfigStore) : SaveAppConfig =
        { Handle = fun request ct -> handle repository appConfigStore request ct }
