namespace AttendanceRecord.Application.Services

open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Domain.Entities
open System.Threading
open R3
open AttendanceRecord.Shared

type AppConfigStore =
   { Current: ReadOnlyReactiveProperty<AppConfig>
     Load: CancellationToken option -> TaskResult<unit, string>
     Set: AppConfig -> unit }

module AppConfigStore =
   let create
      (repository: AppConfigRepository)
      (disposables: CompositeDisposable)
      : AppConfigStore =
      let appConfig = R3.property AppConfig.initial |> R3.disposeWith disposables

      let load (ct: CancellationToken option) : TaskResult<unit, string> =
         taskResult {
            let ct = defaultArg ct CancellationToken.None
            let! config = repository.GetConfig ct
            appConfig.Value <- config
         }

      let set (config: AppConfig) : unit = appConfig.Value <- config

      load None
      |> TaskResult.mapError (fun err -> eprintfn $"アプリ設定の読み込みに失敗しました: {err}")
      |> ignore

      { Current = appConfig
        Load = load
        Set = set }
