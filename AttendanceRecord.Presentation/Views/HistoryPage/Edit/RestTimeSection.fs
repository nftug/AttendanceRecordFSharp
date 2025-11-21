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
        (ctx: HistoryPageContext)
        (handleDelete: Guid option -> unit)
        (item: ReactiveProperty<RestRecordSaveRequestDto>)
        : Avalonia.Controls.Control =
        let handleSetStartedAt (startedAt: DateTime option) : unit =
            item.Value <-
                { item.Value with
                    StartedAt = defaultArg startedAt item.Value.StartedAt }

        let handleSetEndedAt (endedAt: DateTime option) : unit =
            item.Value <- { item.Value with EndedAt = endedAt }

        StackPanel()
            .OrientationHorizontal()
            .Spacing(15.0)
            .Margin(0.0, 0.0, 0.0, 5.0)
            .Children(
                TimePickerField.create
                    { Label = "開始時間"
                      BaseDate = ctx.CurrentDate
                      Value = item |> R3.map (fun r -> Some r.StartedAt)
                      OnSetValue = handleSetStartedAt
                      IsClearable = false },
                TimePickerField.create
                    { Label = "終了時間"
                      BaseDate = ctx.CurrentDate
                      Value = item |> R3.map _.EndedAt
                      OnSetValue = handleSetEndedAt
                      IsClearable = true },
                MaterialIconButton.create
                    { Kind = MaterialIconKind.Delete
                      OnClick = fun _ -> handleDelete item.Value.Id
                      FontSize = Some 20.0
                      Tooltip = Some "休憩時間を削除" }
            )

    let create () =
        withLifecycle (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self

            let restItems =
                R3.list ([]: ReactiveProperty<RestRecordSaveRequestDto> list) disposables

            // Sync from ctx.Form to restItems
            ctx.Form
            |> R3.subscribe (fun formOpt ->
                restItems.Clear()

                match formOpt with
                | Some form -> form.RestRecords |> List.map R3.property |> restItems.AddRange
                | None -> ())
            |> disposables.Add

            // Sync from restItems to ctx.Form
            restItems
            |> R3.mapFromListChanged (fun _ -> ()) ()
            |> R3.subscribe (fun _ ->
                match ctx.Form.Value with
                | Some form ->
                    let updated = restItems |> Seq.map _.Value |> Seq.toList
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
                                    MaterialIconButton.create
                                        { Kind = MaterialIconKind.AddCircleOutline
                                          OnClick = fun _ -> handleAdd ()
                                          FontSize = Some 20.0
                                          Tooltip = Some "休憩時間を追加" }
                                    |> _.Column(2)
                                ),
                            ctx.Form
                            |> toOptView (fun _ _ form ->
                                if form.RestRecords.IsEmpty then
                                    TextBlock()
                                        .Text("休憩記録がありません。")
                                        .FontSize(14.0)
                                        .Foreground(Brushes.Gray)
                                else
                                    StackPanel()
                                        .Spacing(5.0)
                                        .Children(
                                            ItemsControl()
                                                .ItemsSource(restItems)
                                                .ItemTemplate(createRestItemView ctx handleDelete)
                                        ))
                        )
                ))
