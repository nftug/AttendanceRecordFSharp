namespace AttendanceRecord.Application.Services

open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Domain.Entities
open System.Threading

type AppConfigStore(repository: AppConfigRepository) as this =
    let mutable currentConfig = AppConfig.initial

    do
        this.Load()
        |> TaskResult.mapError (fun err -> printfn $"Failed to load AppConfig: {err}")
        |> ignore

    member _.Current: AppConfig = currentConfig

    member _.Load(?ct: CancellationToken) : TaskResult<unit, string> =
        taskResult {
            let ct = defaultArg ct CancellationToken.None
            let! config = repository.GetConfig ct
            currentConfig <- config
        }

    member _.Save(config: AppConfig, ?ct: CancellationToken) : TaskResult<unit, string> =
        taskResult {
            let ct = defaultArg ct CancellationToken.None
            do! repository.SaveConfig config ct
            currentConfig <- config
        }
