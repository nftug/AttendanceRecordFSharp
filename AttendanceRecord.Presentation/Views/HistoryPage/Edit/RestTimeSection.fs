namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open R3
open ObservableCollections
open System
open Avalonia.Media
open Material.Icons
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared

module RestTimeSection =
    open NXUI.Extensions
    open type NXUI.Builders

    let private createRestItemView
        (ctx: HistoryPageContext)
        (handleDelete: Guid option -> unit)
        (items: ObservableList<RestRecordSaveRequestDto>)
        (item: RestRecordSaveRequestDto)
        =
        let index = items.IndexOf item
        let startedAt = item |> R3.everyValueChanged _.StartedAt |> R3.map Some
        let endedAt = item |> R3.everyValueChanged _.EndedAt

        StackPanel()
            .OrientationHorizontal()
            .Spacing(15.0)
            .Margin(0.0, 0.0, 0.0, 10.0)
            .Children(
                TimePickerField.create
                    { Label = "開始時間"
                      BaseDate = ctx.CurrentDate
                      Value = startedAt
                      OnSetValue =
                        fun v ->
                            items[index] <-
                                { item with
                                    StartedAt = defaultArg v item.StartedAt }
                      IsClearable = false },
                TimePickerField.create
                    { Label = "終了時間"
                      BaseDate = ctx.CurrentDate
                      Value = endedAt
                      OnSetValue = fun v -> items[index] <- { item with EndedAt = v }
                      IsClearable = true },
                MaterialIconButton.create
                    { Kind = MaterialIconKind.Delete
                      OnClick = fun _ -> handleDelete item.Id
                      FontSize = Some 18.0
                      Tooltip = Some "休憩時間を削除" }
                |> _.VerticalAlignmentBottom()
            )

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<HistoryPageContext> self

            let restItems = ObservableList<RestRecordSaveRequestDto>()

            // Sync from ctx.Form to restItems
            ctx.Form
            |> R3.map _.RestRecords
            |> R3.subscribe (fun items ->
                restItems.Clear()
                restItems.AddRange items)
            |> disposables.Add

            // Sync from restItems to ctx.Form
            restItems
            |> R3.observeCollection
            |> R3.subscribe (fun _ ->
                ctx.Form.Value <-
                    { ctx.Form.Value with
                        RestRecords = restItems |> Seq.toList })
            |> disposables.Add

            let handleDelete (id: Guid option) =
                match restItems |> Seq.tryFind (fun rp -> rp.Id = id) with
                | Some rp -> restItems.Remove rp |> ignore
                | None -> ()

            let handleAdd () =
                RestRecordSaveRequestDto.empty ctx.Form.Value.StartedAt.Date |> restItems.Add

            let isEmpty = ctx.Form |> R3.map (fun f -> f.RestRecords.IsEmpty)

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(15.0)
                .Child(
                    Grid()
                        .RowDefinitions("Auto,*")
                        .RowSpacing(15.0)
                        .Children(
                            Grid()
                                .ColumnDefinitions("Auto,*,Auto")
                                .Children(
                                    TextBlock()
                                        .Text("休憩")
                                        .FontSize(18.0)
                                        .FontWeightBold()
                                        .Column(0),
                                    MaterialIconButton.create
                                        { Kind = MaterialIconKind.AddCircleOutline
                                          OnClick = fun _ -> handleAdd ()
                                          FontSize = Some 20.0
                                          Tooltip = Some "休憩時間を追加" }
                                    |> _.Column(2)
                                )
                                .Row(0),
                            ScrollViewer()
                                .HorizontalScrollBarVisibilityDisabled()
                                .Content(
                                    ItemsControl()
                                        .ItemsSource(restItems.ToNotifyCollectionChangedSlim())
                                        .ItemTemplate(
                                            createRestItemView ctx handleDelete restItems
                                        )
                                )
                                .IsVisible(isEmpty |> R3.map not |> asBinding)
                                .Row(1),
                            TextBlock()
                                .Text("休憩記録がありません。")
                                .FontSize(14.0)
                                .Foreground(Brushes.Gray)
                                .IsVisible(isEmpty |> asBinding)
                                .Row(1)
                        )
                ))
