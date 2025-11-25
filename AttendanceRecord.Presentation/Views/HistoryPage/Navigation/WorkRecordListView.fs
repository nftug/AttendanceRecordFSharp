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

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<HistoryPageContext> self
            let records = ctx.MonthlyRecords |> R3.map _.WorkRecords

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
                                    .VerticalAlignmentCenter()
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

            ItemsControl()
                .ItemsSource(records |> asBinding)
                .TemplateFunc(fun _ ->
                    Border()
                        .BorderThickness(1.0)
                        .BorderBrush(Brushes.Gray)
                        .Child(ScrollViewer().Content(ItemsPresenter())))
                .ItemTemplateFunc(itemTemplate)
                .MinWidth(200.0))
