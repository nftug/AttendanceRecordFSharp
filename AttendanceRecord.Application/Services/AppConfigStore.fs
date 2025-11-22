namespace AttendanceRecord.Application.Services

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Domain.Entities
open System.Threading
open R3
open AttendanceRecord.Shared

type AppConfigStore(repository: AppConfigRepository) as this =
    let disposable = new CompositeDisposable()

    let appConfig = R3.property AppConfig.initial |> R3.disposeWith disposable

    do
        this.Load()
        |> TaskResult.mapError (fun err -> eprintfn $"Failed to load AppConfig: {err}")
        |> ignore

    member _.Current = appConfig :> ReadOnlyReactiveProperty<AppConfig>

    member _.Load(?ct: CancellationToken) : TaskResult<unit, string> =
        taskResult {
            let ct = defaultArg ct CancellationToken.None
            let! config = repository.GetConfig ct
            appConfig.Value <- config
        }

    member _.Set(config: AppConfig) : unit = appConfig.Value <- config

    interface IDisposable with
        member _.Dispose() = disposable.Dispose()
