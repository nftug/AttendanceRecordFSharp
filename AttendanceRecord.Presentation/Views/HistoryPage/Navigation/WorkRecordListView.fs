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
        match ctx.CurrentDate.CurrentValue with
        | Some currentDate when date.Date = currentDate.Date -> ()
        | _ ->
            invokeTask disposables (fun ct ->
                task {
                    let! shouldProceed = ctx.ConfirmDiscard ct

                    if shouldProceed then
                        ctx.CurrentDate.Value <- Some date
                })
            |> ignore

module WorkRecordListView =
    open NXUI.Extensions
    open type NXUI.Builders
    open Material.Icons
    open Avalonia.Media
    open AttendanceRecord.Application.Dtos.Responses

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun _ self ->
            let ctx, _ = Context.require<HistoryPageContext> self
            let records = ctx.MonthlyRecords |> R3.map _.WorkRecords

            let itemTemplate d _ (item: WorkRecordListItemDto) : Avalonia.Controls.Control =
                let isSelected =
                    ctx.CurrentDate
                    |> R3.map (function
                        | Some date -> date.Date = item.Date.Date
                        | None -> false)

                AccentToggleButton.create isSelected
                |> _.HorizontalAlignmentStretch()
                    .HorizontalContentAlignmentLeft()
                    .Padding(10.0)
                    .FontSize(14.0)
                    .Background(Brushes.Transparent)
                    .BorderBrush(Brushes.Transparent)
                    .OnClickHandler(fun _ _ -> handleDateSelect ctx d item.Date)
                    .Content(
                        StackPanel()
                            .OrientationHorizontal()
                            .Spacing(10.0)
                            .Children(
                                MaterialIcon
                                    .create(MaterialIconKind.CalendarToday |> R3.ret)
                                    .Foreground(Brushes.DarkGray),
                                TextBlock()
                                    .Text(item.Date.ToString "yyyy/MM/dd (ddd)")
                                    .VerticalAlignmentCenter()
                            )
                    )

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Child(
                    ScrollViewer()
                        .VerticalScrollBarVisibilityAuto()
                        .MinWidth(200.0)
                        .Content(records |> toListView itemTemplate)
                ))
