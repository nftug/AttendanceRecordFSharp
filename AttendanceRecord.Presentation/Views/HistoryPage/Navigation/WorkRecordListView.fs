namespace AttendanceRecord.Presentation.Views.HistoryPage.Navigation

open System
open System.Threading
open System.Threading.Tasks
open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared

type WorkRecordListViewProps =
    { OnConfirmDiscard: CancellationToken -> Task<bool> }

[<AutoOpen>]
module private WorkRecordListViewLogic =
    let handleDateSelect
        (ctx: HistoryPageContext)
        (props: WorkRecordListViewProps)
        (disposables: CompositeDisposable)
        (date: DateTime)
        : unit =
        match ctx.CurrentDate.CurrentValue with
        | Some currentDate when date.Date = currentDate.Date -> ()
        | _ ->
            invokeTask disposables (fun ct ->
                task {
                    let! shouldProceed = props.OnConfirmDiscard ct

                    if shouldProceed then
                        ctx.CurrentDate.Value <- Some date
                        ctx.IsFormDirty.Value <- false
                })
            |> ignore

module WorkRecordListView =
    open NXUI.Extensions
    open type NXUI.Builders
    open Material.Icons
    open Avalonia.Media
    open AttendanceRecord.Application.Dtos.Responses
    open System.Collections.ObjectModel

    let create (props: WorkRecordListViewProps) : Avalonia.Controls.Control =
        withReactive (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self

            let items = ObservableCollection<WorkRecordListItemDto> []

            ctx.MonthlyRecords
            |> R3.subscribe (fun records ->
                items.Clear()
                records.WorkRecords |> List.iter items.Add)
            |> disposables.Add

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
                    .OnClickHandler(fun _ _ -> handleDateSelect ctx props disposables item.Date)
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
                        .Content(
                            ItemsControl()
                                .MinWidth(200.0)
                                .ItemsSource(items)
                                .ItemTemplate(itemTemplate)
                        )
                ))
