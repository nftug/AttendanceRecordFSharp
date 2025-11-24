namespace AttendanceRecord.Presentation.Views.HistoryPage.Navigation

open System
open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared

[<AutoOpen>]
module private HistoryToolbarHelpers =
    let handleNavigateMonth
        (ctx: HistoryPageContext)
        (disposables: CompositeDisposable)
        (delta: int)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                let! shouldProceed = ctx.ConfirmDiscard ct

                if shouldProceed then
                    ctx.CurrentMonth.Value <- ctx.CurrentMonth.CurrentValue.AddMonths delta
                    ctx.SelectedDate.Value <- None
            })
        |> ignore

    let private jumpToDate (ctx: HistoryPageContext) (date: DateTime) : unit =
        ctx.CurrentMonth.Value <- DateTime(date.Year, date.Month, 1)
        ctx.SelectedDate.Value <- Some date

    let handleJumpToToday (ctx: HistoryPageContext) (disposables: CompositeDisposable) : unit =
        invokeTask disposables (fun ct ->
            task {
                let! shouldProceed = ctx.ConfirmDiscard ct

                if shouldProceed then
                    jumpToDate ctx DateTime.Today
            })
        |> ignore

    let handleShowDatePicker (ctx: HistoryPageContext) (disposables: CompositeDisposable) : unit =
        invokeTask disposables (fun ct ->
            task {
                let! shouldProceed = ctx.ConfirmDiscard ct

                if shouldProceed then
                    let! result =
                        DatePickerDialog.show
                            { InitialDate = ctx.SelectedDate.CurrentValue
                              InitialMonth = Some ctx.CurrentMonth.CurrentValue }
                            (Some ct)

                    match result with
                    | Some date when result <> ctx.SelectedDate.CurrentValue -> jumpToDate ctx date
                    | _ -> ()
            })
        |> ignore

module HistoryToolbar =
    open NXUI.Extensions
    open type NXUI.Builders
    open Material.Icons

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<HistoryPageContext> self
            let monthText = ctx.CurrentMonth |> R3.map _.ToString("yyyy年 MM月")

            Grid()
                .ColumnDefinitions("Auto,*,Auto")
                .Height(50.0)
                .Margin(0.0, 5.0)
                .Children(
                    StackPanel()
                        .OrientationHorizontal()
                        .Spacing(5.0)
                        .Column(0)
                        .Children(
                            MaterialIconButton.create
                                { Kind = MaterialIconKind.NavigateBefore |> R3.ret
                                  OnClick = fun _ -> handleNavigateMonth ctx disposables -1
                                  FontSize = Some 18.0 |> R3.ret
                                  Tooltip = Some "前の月へ移動" |> R3.ret }
                            |> _.Width(50.0).Height(50.0),
                            MaterialIconButton.create
                                { Kind = MaterialIconKind.NavigateNext |> R3.ret
                                  OnClick = fun _ -> handleNavigateMonth ctx disposables 1
                                  FontSize = Some 18.0 |> R3.ret
                                  Tooltip = Some "次の月へ移動" |> R3.ret }
                            |> _.Width(50.0).Height(50.0),
                            MaterialIconButton.create
                                { Kind = MaterialIconKind.Home |> R3.ret
                                  OnClick = fun _ -> handleJumpToToday ctx disposables
                                  FontSize = Some 18.0 |> R3.ret
                                  Tooltip = Some "今日の記録へ移動" |> R3.ret }
                            |> _.Width(50.0)
                                .Height(50.0)
                                .IsEnabled(
                                    ctx.SelectedDate
                                    |> R3.map (fun d -> d <> Some DateTime.Today)
                                    |> asBinding
                                ),
                            MaterialIconButton.create
                                { Kind = MaterialIconKind.CalendarToday |> R3.ret
                                  OnClick = fun _ -> handleShowDatePicker ctx disposables
                                  FontSize = Some 18.0 |> R3.ret
                                  Tooltip = Some "日付を選択" |> R3.ret }
                            |> _.Width(50.0).Height(50.0)
                        ),
                    StackPanel()
                        .OrientationHorizontal()
                        .VerticalAlignmentCenter()
                        .Margin(0.0, 0.0, 20.0, 0.0)
                        .Spacing(10.0)
                        .Column(2)
                        .Children(
                            MaterialIcon.create (MaterialIconKind.CalendarMonth |> R3.ret),
                            TextBlock()
                                .Text(monthText |> asBinding)
                                .FontSize(16.0)
                                .VerticalAlignmentCenter()
                        )
                ))
