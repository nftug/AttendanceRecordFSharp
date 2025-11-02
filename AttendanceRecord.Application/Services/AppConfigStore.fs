namespace AttendanceRecord.Application.Services

open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Domain.Entities

type AppConfigStore(repository: AppConfigRepository) as this =
    let mutable currentConfig = AppConfig.initial

    do
        this.Load()
        |> TaskResult.mapError (fun err -> printfn $"Failed to load AppConfig: {err}")
        |> ignore

    member _.Current: AppConfig = currentConfig

    member _.Load() : TaskResult<unit, string> =
        taskResult {
            let! config = repository.GetConfig()
            currentConfig <- config
        }

    member _.Save(config: AppConfig) : TaskResult<unit, string> =
        taskResult {
            do! repository.SaveConfig config
            currentConfig <- config
        }
