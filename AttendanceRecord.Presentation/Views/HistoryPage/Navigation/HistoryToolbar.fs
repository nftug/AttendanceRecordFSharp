namespace AttendanceRecord.Presentation.Views.HistoryPage.Navigation

open System
open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context

[<AutoOpen>]
module private HistoryToolbarLogic =
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
                    ctx.CurrentDate.Value <- None
            })
        |> ignore

    let private jumpToDate (ctx: HistoryPageContext) (date: DateTime) : unit =
        ctx.CurrentMonth.Value <- DateTime(date.Year, date.Month, 1)
        ctx.CurrentDate.Value <- Some date

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
                            { InitialDate = ctx.CurrentDate.CurrentValue
                              InitialMonth = Some ctx.CurrentMonth.CurrentValue }
                            (Some ct)

                    match result with
                    | Some date when result <> ctx.CurrentDate.CurrentValue -> jumpToDate ctx date
                    | _ -> ()
            })
        |> ignore

module HistoryToolbar =
    open NXUI.Extensions
    open type NXUI.Builders
    open Material.Icons
    open AttendanceRecord.Shared

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self
            let monthText = ctx.CurrentMonth |> R3.map _.ToString("yyyy年 MM月")

            Grid()
                .ColumnDefinitions("Auto,*,Auto")
                .Height(50.0)
                .Margin(5.0)
                .Children(
                    StackPanel()
                        .OrientationHorizontal()
                        .Spacing(5.0)
                        .Column(0)
                        .Children(
                            MaterialIconButton.create
                                { Kind = MaterialIconKind.NavigateBefore
                                  OnClick = fun _ -> handleNavigateMonth ctx disposables -1
                                  FontSize = Some 18.0
                                  Tooltip = Some "前の月へ移動" }
                            |> _.Width(50.0).Height(50.0),
                            MaterialIconButton.create
                                { Kind = MaterialIconKind.NavigateNext
                                  OnClick = fun _ -> handleNavigateMonth ctx disposables 1
                                  FontSize = Some 18.0
                                  Tooltip = Some "次の月へ移動" }
                            |> _.Width(50.0).Height(50.0),
                            MaterialIconButton.create
                                { Kind = MaterialIconKind.Home
                                  OnClick = fun _ -> handleJumpToToday ctx disposables
                                  FontSize = Some 18.0
                                  Tooltip = Some "今日の記録へ移動" }
                            |> _.Width(50.0).Height(50.0),
                            MaterialIconButton.create
                                { Kind = MaterialIconKind.CalendarToday
                                  OnClick = fun _ -> handleShowDatePicker ctx disposables
                                  FontSize = Some 18.0
                                  Tooltip = Some "日付を選択" }
                            |> _.Width(50.0).Height(50.0)
                        ),
                    StackPanel()
                        .OrientationHorizontal()
                        .VerticalAlignmentCenter()
                        .Spacing(10.0)
                        .Margin(8.0, 0.0)
                        .Column(2)
                        .Children(
                            MaterialIcon.create MaterialIconKind.CalendarMonth,
                            TextBlock()
                                .Text(monthText |> asBinding)
                                .FontSize(16.0)
                                .VerticalAlignmentCenter()
                        )
                ))
