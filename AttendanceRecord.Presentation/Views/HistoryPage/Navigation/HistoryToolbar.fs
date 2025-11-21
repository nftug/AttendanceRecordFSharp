namespace AttendanceRecord.Presentation.Views.HistoryPage.Navigation

open System
open System.Threading
open System.Threading.Tasks
open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context

type HistoryToolbarProps =
    { OnConfirmDiscard: CancellationToken -> Task<bool> }

[<AutoOpen>]
module private HistoryToolbarLogic =
    let handleNavigateMonth
        (ctx: HistoryPageContext)
        (props: HistoryToolbarProps)
        (disposables: CompositeDisposable)
        (delta: int)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                let! shouldProceed = props.OnConfirmDiscard ct

                if shouldProceed then
                    ctx.CurrentMonth.Value <- ctx.CurrentMonth.CurrentValue.AddMonths delta
                    ctx.CurrentDate.Value <- None
            })
        |> ignore

    let handleJumpToToday
        (ctx: HistoryPageContext)
        (props: HistoryToolbarProps)
        (disposables: CompositeDisposable)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                let! shouldProceed = props.OnConfirmDiscard ct

                if shouldProceed then
                    let today = DateTime.Now.Date
                    ctx.CurrentMonth.Value <- DateTime(today.Year, today.Month, 1)
                    ctx.CurrentDate.Value <- Some today
            })
        |> ignore

    let handleShowDatePicker
        (ctx: HistoryPageContext)
        (props: HistoryToolbarProps)
        (disposables: CompositeDisposable)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                let! shouldProceed = props.OnConfirmDiscard ct

                if shouldProceed then
                    let! result =
                        DatePickerDialog.show
                            { InitialDate = ctx.CurrentDate.CurrentValue
                              InitialMonth = Some ctx.CurrentMonth.CurrentValue }
                            (Some ct)

                    match result with
                    | Some date when result <> ctx.CurrentDate.CurrentValue ->
                        ctx.CurrentDate.Value <- Some date
                        ctx.CurrentMonth.Value <- DateTime(date.Year, date.Month, 1)
                    | _ -> ()
            })
        |> ignore

module HistoryToolbar =
    open NXUI.Extensions
    open type NXUI.Builders
    open Avalonia.Media
    open Material.Icons
    open AttendanceRecord.Shared

    let create (props: HistoryToolbarProps) : Avalonia.Controls.Control =
        withReactive (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self
            let monthText = ctx.CurrentMonth |> R3.map (fun d -> d.ToString "yyyy年 MM月")

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
                            (MaterialIconButton.create
                                { Kind = MaterialIconKind.NavigateBefore
                                  OnClick = fun _ -> handleNavigateMonth ctx props disposables -1
                                  FontSize = Some 18.0
                                  Tooltip = Some "前の月へ移動" })
                                .Width(50.0)
                                .Height(50.0),
                            (MaterialIconButton.create
                                { Kind = MaterialIconKind.NavigateNext
                                  OnClick = fun _ -> handleNavigateMonth ctx props disposables 1
                                  FontSize = Some 18.0
                                  Tooltip = Some "次の月へ移動" })
                                .Width(50.0)
                                .Height(50.0),
                            (MaterialIconButton.create
                                { Kind = MaterialIconKind.Home
                                  OnClick = fun _ -> handleJumpToToday ctx props disposables
                                  FontSize = Some 18.0
                                  Tooltip = Some "今月へ移動" })
                                .Width(50.0)
                                .Height(50.0),
                            (MaterialIconButton.create
                                { Kind = MaterialIconKind.CalendarToday
                                  OnClick = fun _ -> handleShowDatePicker ctx props disposables
                                  FontSize = Some 18.0
                                  Tooltip = Some "日付を選択" })
                                .Width(50.0)
                                .Height(50.0)
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
