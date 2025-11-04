namespace AttendanceRecord.Presentation.Features.HomePage.Components

open System.Threading.Tasks
open FsToolkit.ErrorHandling
open Avalonia
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Controls.Primitives
open Avalonia.Controls.Notifications
open Material.Icons
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Presentation.Common.Components

type HomeActionsViewProps =
    { Status: IReadable<CurrentStatusDto>
      OnToggleWork: unit -> TaskResult<CurrentStatusDto, string>
      OnToggleRest: unit -> TaskResult<CurrentStatusDto, string> }

module HomeActionsView =
    let view (props: HomeActionsViewProps) =
        Component.create (
            "HomeActionsView",
            fun ctx ->
                let status = props.Status |> ctx.usePassedRead
                let toggleWorkMutation = ctx.useMutation props.OnToggleWork
                let toggleRestMutation = ctx.useMutation props.OnToggleRest

                let workButtonLabel =
                    ctx.useDerived1 (status, (fun s -> if s.IsActive then "勤務終了" else "勤務開始"))

                let restButtonLabel =
                    ctx.useDerived1 (status, (fun s -> if s.IsResting then "休憩終了" else "休憩開始"))

                let workToggleEnabled =
                    ctx.useDerived1 (toggleWorkMutation.IsPending, (fun isPending -> not isPending))

                let restToggleEnabled =
                    ctx.useDerived2 ((toggleRestMutation.IsPending, status), (fun (r, s) -> not r && s.IsActive))

                let handleClickToggleWork () =
                    task {
                        let! confirmed =
                            if status.Current.IsActive then
                                ConfirmDialogView.show
                                    { Title = "勤務終了の確認"
                                      Message = "勤務を終了しますか？" }
                            else
                                true |> Task.FromResult

                        if not confirmed then
                            return ()
                        else
                            let! result = toggleWorkMutation.MutateTask()

                            match result with
                            | Ok status ->
                                let message = if status.IsActive then "勤務を開始しました。" else "勤務を終了しました。"

                                Notification.showWindowNotification "勤怠状態更新" message NotificationType.Information
                            | Error error -> Notification.showWindowNotification "勤怠状態更新" error NotificationType.Error
                    }
                    |> ignore

                let handleClickToggleRest () =
                    task {
                        let! result = toggleRestMutation.MutateTask()

                        match result with
                        | Ok status ->
                            let message = if status.IsResting then "休憩を開始しました。" else "休憩を終了しました。"

                            Notification.showWindowNotification "勤怠状態更新" message NotificationType.Information
                        | Error error -> Notification.showWindowNotification "勤怠状態更新" error NotificationType.Error
                    }
                    |> ignore

                Grid.create
                    [ Grid.rowDefinitions "Auto"
                      Grid.columnDefinitions "*,*"
                      Grid.children
                          [ ToggleButton.create
                                [ Grid.column 0
                                  ToggleButton.content (
                                      MaterialIconLabel.create
                                          MaterialIconKind.Work
                                          [ CjkTextBlock.create [ TextBlock.text workButtonLabel.Current ] ]
                                  )
                                  ToggleButton.onClick (fun _ -> handleClickToggleWork ())
                                  ToggleButton.isEnabled workToggleEnabled.Current
                                  ToggleButton.isChecked status.Current.IsWorking
                                  ToggleButton.onIsCheckedChanged (fun e ->
                                      let btn = e.Source :?> ToggleButton
                                      btn.IsChecked <- status.Current.IsWorking)
                                  ToggleButton.height 46.0
                                  ToggleButton.fontSize 16.0
                                  ToggleButton.horizontalAlignment HorizontalAlignment.Stretch
                                  ToggleButton.margin (Thickness(0, 0, 10.0, 0)) ]
                            ToggleButton.create
                                [ Grid.column 1
                                  ToggleButton.content (
                                      MaterialIconLabel.create
                                          MaterialIconKind.Coffee
                                          [ CjkTextBlock.create [ TextBlock.text restButtonLabel.Current ] ]
                                  )
                                  ToggleButton.onClick (fun _ -> handleClickToggleRest ())
                                  ToggleButton.isEnabled restToggleEnabled.Current
                                  ToggleButton.isChecked status.Current.IsResting
                                  ToggleButton.onIsCheckedChanged (fun e ->
                                      let btn = e.Source :?> ToggleButton
                                      btn.IsChecked <- status.Current.IsResting)
                                  ToggleButton.height 46.0
                                  ToggleButton.fontSize 16.0
                                  ToggleButton.horizontalAlignment HorizontalAlignment.Stretch ] ] ]

        )
