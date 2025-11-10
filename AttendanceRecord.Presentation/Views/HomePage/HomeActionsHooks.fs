namespace AttendanceRecord.Presentation.Views.HomePage

open R3
open System.Threading.Tasks
open System.Threading
open Material.Icons
open Avalonia.Controls.Notifications
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Application.Dtos.Responses

type HomeActionsHooksProps =
    { Status: Observable<CurrentStatusDto>
      OnToggleWork: unit -> CancellationToken -> Task<Result<CurrentStatusDto, string>>
      OnToggleRest: unit -> CancellationToken -> Task<Result<CurrentStatusDto, string>> }

type HomeActionsHooks =
    { Status: ReadOnlyReactiveProperty<CurrentStatusDto>
      WorkButtonLabel: Observable<string>
      RestButtonLabel: Observable<string>
      WorkToggleEnabled: Observable<bool>
      RestToggleEnabled: Observable<bool>
      HandleClickToggleWork: unit -> unit
      HandleClickToggleRest: unit -> unit }

[<AutoOpen>]
module HomeActionsHooks =
    let useHomeActionsHooks
        (props: HomeActionsHooksProps)
        (disposables: CompositeDisposable)
        : HomeActionsHooks =
        let toggleWorkMutation = useMutation disposables props.OnToggleWork
        let toggleRestMutation = useMutation disposables props.OnToggleRest

        let status = props.Status |> R3.readonly None |> R3.disposeWith disposables

        let workButtonLabel =
            status |> R3.map (fun s -> if s.IsActive then "勤務終了" else "勤務開始")

        let restButtonLabel =
            status |> R3.map (fun s -> if s.IsResting then "休憩終了" else "休憩開始")

        let workToggleEnabled =
            toggleWorkMutation.IsPending |> R3.combineLatest2 status (fun r s -> not r)

        let restToggleEnabled =
            toggleRestMutation.IsPending
            |> R3.combineLatest2 status (fun r s -> not r && s.IsActive)

        let handleClickToggleWork () =
            invokeTask disposables (fun ct ->
                task {
                    let! shouldProceed =
                        if not status.CurrentValue.IsActive then
                            task { return true }
                        else
                            MessageBox.show
                                { Title = "勤務終了の確認"
                                  Message = "勤務を終了しますか？"
                                  OkContent = None
                                  CancelContent = None
                                  Buttons = MessageBoxButtons.OkCancel }
                                (Some ct)

                    if shouldProceed then
                        match! toggleWorkMutation.MutateTask () ct with
                        | Ok status ->
                            let message = if status.IsActive then "勤務を開始しました。" else "勤務を終了しました。"
                            Notification.show "勤務状態更新" message NotificationType.Information
                        | Error e ->
                            Notification.show
                                "勤務状態更新エラー"
                                $"勤務状態の更新に失敗しました: {e}"
                                NotificationType.Error
                })
            |> ignore

        let handleClickToggleRest () =
            invokeTask disposables (fun ct ->
                task {
                    match! toggleRestMutation.MutateTask () ct with
                    | Ok status ->
                        let message = if status.IsResting then "休憩を開始しました。" else "休憩を終了しました。"
                        Notification.show "休憩状態更新" message NotificationType.Information
                    | Error e ->
                        Notification.show "休憩状態更新エラー" $"休憩状態の更新に失敗しました: {e}" NotificationType.Error
                })
            |> ignore

        { Status = status
          WorkButtonLabel = workButtonLabel
          RestButtonLabel = restButtonLabel
          WorkToggleEnabled = workToggleEnabled
          RestToggleEnabled = restToggleEnabled
          HandleClickToggleWork = handleClickToggleWork
          HandleClickToggleRest = handleClickToggleRest }
