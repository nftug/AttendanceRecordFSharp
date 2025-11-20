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
        (onDelete: Guid option -> unit)
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
                let s = defaultArg s item.Value.StartedAt

                item.Value <-
                    { item.Value with
                        StartedAt = s
                        EndedAt = e })
            |> disposables.Add

            StackPanel()
                .OrientationHorizontal()
                .Spacing(10.0)
                .Margin(0.0, 0.0, 0.0, 5.0)
                .Children(
                    TimePickerField.create
                        { Label = "開始時間"
                          BaseDate = ctx.CurrentDate
                          SelectedDateTime = startedAt
                          IsDirty = Some ctx.IsFormDirty },
                    TimePickerField.create
                        { Label = "終了時間"
                          BaseDate = ctx.CurrentDate
                          SelectedDateTime = endedAt
                          IsDirty = Some ctx.IsFormDirty },
                    Button()
                        .Content(MaterialIcon.create MaterialIconKind.Delete)
                        .OnClickHandler(fun _ _ -> onDelete item.Value.Id)
                        .Width(40.0)
                        .Height(40.0)
                        .Background(Brushes.Transparent)
                        .BorderBrush(Brushes.Transparent)
                ))

    let private createRestTimesContent () =
        withReactive (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self

            ctx.Form
            |> toView (fun recordOpt ->
                match recordOpt with
                | None -> Panel()
                | Some record when record.RestRecords.IsEmpty ->
                    TextBlock().Text("休憩記録がありません。").FontSize(14.0).Foreground(Brushes.Gray)
                | Some _ ->
                    let restItems =
                        R3.collection ([]: ReactiveProperty<RestRecordSaveRequestDto> list)

                    ctx.Form
                    |> R3.subscribe (fun rOpt ->
                        match rOpt with
                        | Some r ->
                            restItems.Clear()

                            r.RestRecords
                            |> List.map (fun rt -> R3.property rt |> R3.disposeWith disposables)
                            |> List.iter restItems.Add
                        | None -> ())
                    |> disposables.Add

                    restItems
                    |> R3.mapFromCollectionChanged (fun _ -> ()) ()
                    |> R3.subscribe (fun _ ->
                        match ctx.Form.Value with
                        | Some r ->
                            let updated = restItems |> Seq.map (fun rp -> rp.Value) |> Seq.toList

                            ctx.Form.Value <- Some { r with RestRecords = updated }
                        | None -> ())
                    |> disposables.Add

                    let onDelete (id: Guid option) =
                        let toRemove = restItems |> Seq.tryFind (fun rp -> rp.Value.Id = id)

                        match toRemove with
                        | Some rp -> restItems.Remove rp |> ignore
                        | None -> ()

                    StackPanel()
                        .Spacing(5.0)
                        .Children(
                            ItemsControl()
                                .ItemsSource(restItems)
                                .ItemTemplate(createRestItemView onDelete)
                        )))

    let create () =
        withReactive (fun _ self ->
            let ctx, _ = HistoryPageContextProvider.require self

            let handleAddRestTime () =
                match ctx.Form.Value with
                | Some r ->
                    let baseDate = r.StartedAt.Date

                    let updated =
                        { r with
                            RestRecords =
                                r.RestRecords @ [ RestRecordSaveRequestDto.empty baseDate ] }

                    ctx.Form.Value <- Some updated
                    ctx.IsFormDirty.Value <- true
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
                                    Button()
                                        .Content("+ 追加")
                                        .OnClickHandler(fun _ _ -> handleAddRestTime ())
                                        .Column(2)
                                ),
                            createRestTimesContent ()
                        )
                ))
