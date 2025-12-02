namespace AttendanceRecord.Presentation.Views.HistoryPage.Navigation

open System
open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared

[<AutoOpen>]
module private WorkRecordListViewHelpers =
    let handleDateSelect
        (ctx: HistoryPageContext)
        (disposables: CompositeDisposable)
        (date: DateTime)
        : unit =
        match ctx.SelectedDate.CurrentValue with
        | Some currentDate when date.Date = currentDate.Date -> ()
        | _ ->
            invokeTask disposables (fun ct ->
                task {
                    let! shouldProceed = ctx.ConfirmDiscard ct

                    if shouldProceed then
                        ctx.SelectedDate.Value <- Some date
                })
            |> ignore

module WorkRecordListView =
    open NXUI.Extensions
    open type NXUI.Builders
    open Avalonia.Media
    open AttendanceRecord.Application.Dtos.Responses
    open FluentAvalonia.UI.Controls
    open AttendanceRecord.Presentation.Views.Common.Context

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<HistoryPageContext> self
            let records = ctx.MonthlyRecords |> R3.map _.WorkRecords
            let themeCtx = Context.require<ThemeContext> self |> fst

            let itemTemplate (item: WorkRecordListItemDto) =
                let isSelected =
                    ctx.SelectedDate
                    |> R3.map (function
                        | Some date -> date.Date = item.Date.Date
                        | None -> false)

                (AccentToggleButton.create isSelected)
                    .Content(
                        StackPanel()
                            .OrientationHorizontal()
                            .Spacing(10.0)
                            .Children(
                                (SymbolIcon.create (Symbol.CalendarDay |> R3.ret))
                                    .Foreground(Brushes.DarkGray),
                                TextBlock()
                                    .Text(item.Date.ToString "yyyy/MM/dd (ddd)")
                                    .VerticalAlignmentCenter(),
                                (SymbolIcon.create (enum<Symbol> 0xE7BA |> R3.ret))
                                    .Foreground(
                                        themeCtx.GetBrushResourceObservable
                                            "SystemFillColorCriticalBrush"
                                        |> asBinding
                                    )
                                    .IsVisible(item.HasUnfinishedWarning |> R3.ret |> asBinding)
                            )
                    )
                    .OnClickHandler(fun _ _ -> handleDateSelect ctx disposables item.Date)
                    .HorizontalAlignmentStretch()
                    .HorizontalContentAlignmentLeft()
                    .Background(Brushes.Transparent)
                    .BorderBrush(Brushes.Transparent)
                    .CornerRadius(0.0)
                    .Padding(10.0)
                    .FontSize(14.0)
                    .Tip(
                        match item.HasUnfinishedWarning with
                        | true -> "未完了の項目があります。"
                        | false -> null
                    )

            ItemsControl()
                .ItemsSource(records |> asBinding)
                .TemplateFunc(fun _ ->
                    Border()
                        .BorderThickness(1.0)
                        .BorderBrush(
                            themeCtx.GetBrushResourceObservable "ControlStrokeColorDefaultBrush"
                            |> asBinding
                        )
                        .Background(
                            themeCtx.GetBrushResourceObservable "ControlFillColorDefaultBrush"
                            |> asBinding
                        )
                        .Padding(5.0)
                        .Child(ScrollViewer().Content(ItemsPresenter())))
                .ItemTemplateFunc(itemTemplate)
                .MinWidth(200.0))
