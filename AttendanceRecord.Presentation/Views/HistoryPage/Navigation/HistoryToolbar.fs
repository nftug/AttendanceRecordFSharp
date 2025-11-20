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
        ()
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
        ()
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
                            Button()
                                .Content(MaterialIcon.create MaterialIconKind.NavigateBefore)
                                .OnClickHandler(fun _ _ ->
                                    handleNavigateMonth ctx props disposables -1)
                                .Width(50.0)
                                .Height(50.0)
                                .FontSize(18.0)
                                .Background(Brushes.Transparent)
                                .BorderBrush(Brushes.Transparent),
                            Button()
                                .Content(MaterialIcon.create MaterialIconKind.NavigateNext)
                                .OnClickHandler(fun _ _ ->
                                    handleNavigateMonth ctx props disposables 1)
                                .Width(50.0)
                                .Height(50.0)
                                .FontSize(18.0)
                                .Background(Brushes.Transparent)
                                .BorderBrush(Brushes.Transparent),
                            Button()
                                .Content(MaterialIcon.create MaterialIconKind.Home)
                                .OnClickHandler(fun _ _ ->
                                    handleJumpToToday ctx props disposables ())
                                .Width(50.0)
                                .Height(50.0)
                                .FontSize(18.0)
                                .Background(Brushes.Transparent)
                                .BorderBrush(Brushes.Transparent),
                            Button()
                                .Content(MaterialIcon.create MaterialIconKind.CalendarToday)
                                .OnClickHandler(fun _ _ ->
                                    handleShowDatePicker ctx props disposables ())
                                .Width(50.0)
                                .Height(50.0)
                                .FontSize(18.0)
                                .Background(Brushes.Transparent)
                                .BorderBrush(Brushes.Transparent)
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
