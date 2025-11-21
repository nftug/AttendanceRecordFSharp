namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open R3
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
        (handleDelete: Guid option -> unit)
        (item: ReactiveProperty<RestRecordSaveRequestDto>)
        : Avalonia.Controls.Control =
        withReactive (fun disposables self ->
            let startedAt = R3.property (None: DateTime option) |> R3.disposeWith disposables
            let endedAt = R3.property (None: DateTime option) |> R3.disposeWith disposables
            let ctx, _ = HistoryPageContextProvider.require self

            item
            |> R3.subscribe (fun r ->
                startedAt.Value <- Some r.StartedAt
                endedAt.Value <- r.EndedAt)
            |> disposables.Add

            startedAt
            |> R3.combineLatest2 endedAt (fun sa ea -> sa, ea)
            |> R3.subscribe (fun (s, e) ->
                item.Value <-
                    { item.Value with
                        StartedAt = defaultArg s item.Value.StartedAt
                        EndedAt = e })
            |> disposables.Add

            StackPanel()
                .OrientationHorizontal()
                .Spacing(15.0)
                .Margin(0.0, 0.0, 0.0, 5.0)
                .Children(
                    TimePickerField.create
                        { Label = "開始時間"
                          BaseDate = ctx.CurrentDate
                          SelectedDateTime = startedAt
                          IsClearable = false },
                    TimePickerField.create
                        { Label = "終了時間"
                          BaseDate = ctx.CurrentDate
                          SelectedDateTime = endedAt
                          IsClearable = true },
                    MaterialIconButton.create
                        { Kind = MaterialIconKind.Delete
                          OnClick = fun _ -> handleDelete item.Value.Id
                          FontSize = Some 20.0
                          Tooltip = Some "休憩時間を削除" }
                ))

    let create () =
        withReactive (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self

            let restItems =
                R3.list ([]: ReactiveProperty<RestRecordSaveRequestDto> list) disposables

            ctx.Form
            |> R3.subscribe (fun formOpt ->
                restItems.Clear()

                match formOpt with
                | Some form -> form.RestRecords |> List.map R3.property |> restItems.AddRange
                | None -> ())
            |> disposables.Add

            restItems
            |> R3.mapFromListChanged (fun _ -> ()) ()
            |> R3.subscribe (fun _ ->
                match ctx.Form.Value with
                | Some form ->
                    let updated = restItems |> Seq.map (fun rp -> rp.Value) |> Seq.toList
                    ctx.Form.Value <- Some { form with RestRecords = updated }
                | None -> ())
            |> disposables.Add

            let handleDelete (id: Guid option) =
                match restItems |> Seq.tryFind (fun rp -> rp.Value.Id = id) with
                | Some rp -> restItems.Remove rp |> ignore
                | None -> ()

            let handleAdd () =
                match ctx.Form.Value with
                | Some form ->
                    RestRecordSaveRequestDto.empty form.StartedAt.Date
                    |> R3.property
                    |> restItems.Add
                | None -> ()

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(15.0)
                .Child(
                    StackPanel()
                        .Spacing(15.0)
                        .Children(
                            Grid()
                                .ColumnDefinitions("Auto,*,Auto")
                                .Children(
                                    TextBlock()
                                        .Text("休憩")
                                        .FontSize(18.0)
                                        .FontWeightBold()
                                        .Column(0),
                                    (MaterialIconButton.create
                                        { Kind = MaterialIconKind.AddCircleOutline
                                          OnClick = fun _ -> handleAdd ()
                                          FontSize = Some 20.0
                                          Tooltip = Some "休憩時間を追加" })
                                        .Column(2)
                                ),
                            ctx.Form
                            |> toView (fun formOpt ->
                                match formOpt with
                                | None -> Panel()
                                | Some record when record.RestRecords.IsEmpty ->
                                    TextBlock()
                                        .Text("休憩記録がありません。")
                                        .FontSize(14.0)
                                        .Foreground(Brushes.Gray)
                                | Some _ ->
                                    StackPanel()
                                        .Spacing(5.0)
                                        .Children(
                                            ItemsControl()
                                                .ItemsSource(restItems)
                                                .ItemTemplate(createRestItemView handleDelete)
                                        ))
                        )
                ))
