namespace AttendanceRecord.Presentation.Views.HistoryPage.Navigation

open System
open System.Threading
open System.Threading.Tasks
open R3
open AttendanceRecord.Presentation.Utils
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
        (date: DateTime option)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                let! shouldProceed = props.OnConfirmDiscard ct

                if shouldProceed then
                    ctx.CurrentDate.Value <- date
            })
        |> ignore

module WorkRecordListView =
    open NXUI.Extensions
    open type NXUI.Builders
    open Material.Icons
    open AttendanceRecord.Application.Dtos.Responses
    open System.Collections.ObjectModel

    let create (props: WorkRecordListViewProps) : Avalonia.Controls.Control =
        withReactive (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self

            let items = ObservableCollection<WorkRecordListItemDto> []
            let itemsCount = items |> R3.collectionChanged

            ctx.MonthlyRecords
            |> R3.subscribe (fun records ->
                items.Clear()
                records.WorkRecords |> List.iter items.Add)
            |> disposables.Add

            let selectedItem =
                ctx.CurrentDate
                |> R3.combineLatest2 itemsCount (fun date _ -> date)
                |> R3.map (fun date ->
                    match date with
                    | Some date -> items |> Seq.tryFind (fun item -> item.Date.Date = date.Date)
                    | None -> None)

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Avalonia.Media.Brushes.Gray)
                .Child(
                    ScrollViewer()
                        .VerticalScrollBarVisibilityAuto()
                        .Content(
                            ListBox()
                                .MinWidth(200.0)
                                .HorizontalAlignmentStretch()
                                .SelectedItem(selectedItem |> R3.map Option.toObj |> asBinding)
                                .OnSelectionChangedHandler(fun lb _ ->
                                    match lb.SelectedItem with
                                    | :? WorkRecordListItemDto as item -> Some item.Date
                                    | _ -> None
                                    |> handleDateSelect ctx props disposables)
                                .ItemsSource(items)
                                .ItemTemplate(fun (item: WorkRecordListItemDto) ->
                                    StackPanel()
                                        .OrientationHorizontal()
                                        .Spacing(10.0)
                                        .Children(
                                            MaterialIcon
                                                .create(MaterialIconKind.CalendarToday)
                                                .FontSize(14.0)
                                                .Foreground(Avalonia.Media.Brushes.DimGray),
                                            TextBlock()
                                                .Text(item.Date.ToString "yyyy/MM/dd (ddd)")
                                                .FontSize(14.0)
                                                .VerticalAlignmentCenter()
                                        )
                                    :> Avalonia.Controls.Control)
                        )
                ))
