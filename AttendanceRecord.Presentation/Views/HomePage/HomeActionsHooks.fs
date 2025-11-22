namespace AttendanceRecord.Presentation.Views.HomePage

open R3
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Presentation.Views.HomePage.Context

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
        (ctx: HomePageContext)
        (disposables: CompositeDisposable)
        : HomeActionsHooks =
        let toggleWorkMutation = useMutation disposables ctx.ToggleWork.Handle
        let toggleRestMutation = useMutation disposables ctx.ToggleRest.Handle

        let status = ctx.Status |> R3.readonly None |> R3.disposeWith disposables

        let workButtonLabel =
            status |> R3.map (fun s -> if s.IsActive then "勤務終了" else "勤務開始")

        let restButtonLabel =
            status |> R3.map (fun s -> if s.IsResting then "休憩終了" else "休憩開始")

        let workToggleEnabled = toggleWorkMutation.IsPending |> R3.map not

        let restToggleEnabled =
            R3.combineLatest2 toggleRestMutation.IsPending status
            |> R3.map (fun (isPending, status) -> not isPending && status.IsActive)

        let handleClickToggleWork () : unit =
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

                            Notification.show
                                { Title = "勤務状態更新"
                                  Message = message
                                  NotificationType = NotificationType.Information }

                        | Error e ->
                            Notification.show
                                { Title = "勤務状態更新エラー"
                                  Message = $"勤務状態の更新に失敗しました: {e}"
                                  NotificationType = NotificationType.Error }
                })
            |> ignore

        let handleClickToggleRest () : unit =
            invokeTask disposables (fun ct ->
                task {
                    match! toggleRestMutation.MutateTask () ct with
                    | Ok status ->
                        let message = if status.IsResting then "休憩を開始しました。" else "休憩を終了しました。"

                        Notification.show
                            { Title = "休憩状態更新"
                              Message = message
                              NotificationType = NotificationType.Information }
                    | Error e ->
                        Notification.show
                            { Title = "休憩状態更新エラー"
                              Message = $"休憩状態の更新に失敗しました: {e}"
                              NotificationType = NotificationType.Error }
                })
            |> ignore

        { Status = status
          WorkButtonLabel = workButtonLabel
          RestButtonLabel = restButtonLabel
          WorkToggleEnabled = workToggleEnabled
          RestToggleEnabled = restToggleEnabled
          HandleClickToggleWork = handleClickToggleWork
          HandleClickToggleRest = handleClickToggleRest }
