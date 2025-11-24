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
        (handleDelete: Guid -> unit)
        (items: ObservableList<RestRecordSaveRequestDto>)
        (item: RestRecordSaveRequestDto)
        =
        let update (updater: RestRecordSaveRequestDto -> RestRecordSaveRequestDto) =
            let index = items |> Seq.findIndex (fun rp -> rp.Id = item.Id)
            items[index] <- updater item

        let startedAt = item |> R3.everyValueChanged _.StartedAt |> R3.map Some
        let endedAt = item |> R3.everyValueChanged _.EndedAt

        StackPanel()
            .OrientationHorizontal()
            .Spacing(15.0)
            .Margin(0.0, 0.0, 0.0, 10.0)
            .Children(
                TimePickerField.create
                    { Label = "開始時間" |> R3.ret
                      BaseDate = ctx.CurrentDate
                      Value = startedAt
                      OnSetValue =
                        fun v ->
                            update (fun rp ->
                                { rp with
                                    StartedAt = defaultArg v rp.StartedAt })
                      IsClearable = false |> R3.ret },
                TimePickerField.create
                    { Label = "終了時間" |> R3.ret
                      BaseDate = ctx.CurrentDate
                      Value = endedAt
                      OnSetValue = fun v -> update (fun rp -> { rp with EndedAt = v })
                      IsClearable = true |> R3.ret },
                MaterialIconButton.create
                    { Kind = MaterialIconKind.Delete |> R3.ret
                      OnClick = fun _ -> handleDelete item.Id
                      FontSize = Some 18.0 |> R3.ret
                      Tooltip = Some "休憩時間を削除" |> R3.ret }
                |> _.VerticalAlignmentBottom()
            )

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<HistoryPageContext> self

            let restItems = ObservableList<RestRecordSaveRequestDto>()

            ctx.ResetCommand
            |> R3.prepend ctx.DefaultForm.CurrentValue
            |> R3.subscribe (fun form ->
                restItems.Clear()
                restItems.AddRange form.RestRecords)
            |> disposables.Add

            // Sync from restItems to ctx.Form
            restItems
            |> R3.collectionChanged
            |> R3.subscribe (fun _ ->
                ctx.Form.Value <-
                    { ctx.Form.Value with
                        RestRecords = restItems |> Seq.toList })
            |> disposables.Add

            let handleDelete (id: Guid) =
                match restItems |> Seq.tryFind (fun rp -> rp.Id = id) with
                | Some rp -> restItems.Remove rp |> ignore
                | None -> ()

            let addCommand =
                R3.command ()
                |> R3.withSubscribe disposables (fun () ->
                    RestRecordSaveRequestDto.empty ctx.Form.Value.StartedAt.Date |> restItems.Add)

            let buildContent () =
                ctx.Form
                |> R3.map _.RestRecords.IsEmpty
                |> R3.distinctUntilChanged
                |> toView (fun disposables _ hasNoItems ->
                    if hasNoItems then
                        TextBlock()
                            .Text("休憩記録がありません。")
                            .FontSize(14.0)
                            .Foreground(Brushes.Gray)
                            .Row(1)
                    else
                        ItemsControl()
                            .ItemsSourceObservable(restItems)
                            .ItemsPanelFunc(fun () -> VirtualizingStackPanel())
                            .TemplateFunc(fun () ->
                                let sv =
                                    ScrollViewer()
                                        .VerticalScrollBarVisibilityAuto()
                                        .HorizontalScrollBarVisibilityDisabled()
                                        .Content(ItemsPresenter())

                                ctx.ResetCommand
                                |> R3.subscribe (fun _ -> sv.ScrollToHome())
                                |> disposables.Add

                                addCommand
                                |> R3.subscribe (fun _ -> nextTick (fun () -> sv.ScrollToEnd()))
                                |> disposables.Add

                                sv)
                            .ItemTemplateFunc(createRestItemView ctx handleDelete restItems))

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
                                        { Kind = MaterialIconKind.AddCircleOutline |> R3.ret
                                          OnClick = fun _ -> addCommand.Execute()
                                          FontSize = Some 20.0 |> R3.ret
                                          Tooltip = Some "休憩時間を追加" |> R3.ret }
                                    |> _.Column(2)
                                )
                                .Row(0),
                            buildContent () |> _.Row(1)
                        )
                ))
