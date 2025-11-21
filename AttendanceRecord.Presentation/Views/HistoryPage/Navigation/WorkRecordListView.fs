namespace AttendanceRecord.Presentation.Views.HistoryPage.Navigation

open System
open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared

[<AutoOpen>]
module private WorkRecordListViewLogic =
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
        withReactive (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self

            let itemTemplate (item: WorkRecordListItemDto) : Avalonia.Controls.Control =
                let isSelected =
                    ctx.CurrentDate
                    |> R3.map (fun currentDate ->
                        match currentDate with
                        | Some date -> date.Date = item.Date.Date
                        | None -> false)
                    |> R3.readonly None
                    |> R3.disposeWith disposables

                (AccentToggleButton.create isSelected)
                    .HorizontalAlignmentStretch()
                    .HorizontalContentAlignmentLeft()
                    .Padding(10.0)
                    .FontSize(14.0)
                    .Background(Brushes.Transparent)
                    .BorderBrush(Brushes.Transparent)
                    .OnClickHandler(fun _ _ -> handleDateSelect ctx disposables item.Date)
                    .Content(
                        StackPanel()
                            .OrientationHorizontal()
                            .Spacing(10.0)
                            .Children(
                                MaterialIcon
                                    .create(MaterialIconKind.CalendarToday)
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
                        .Content(
                            ctx.MonthlyRecords
                            |> R3.map (fun records -> records.WorkRecords)
                            |> toListView itemTemplate
                        )
                ))
