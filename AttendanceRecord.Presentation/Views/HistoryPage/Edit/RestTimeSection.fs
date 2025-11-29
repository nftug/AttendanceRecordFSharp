namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open R3
open ObservableCollections
open System
open Avalonia.Media
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.Dtos.Enums
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared

module RestTimeSection =
    open NXUI.Extensions
    open type NXUI.Builders
    open FluentAvalonia.UI.Controls

    let private createRestItemView
        (ctx: HistoryPageContext)
        (items: ObservableList<RestRecordSaveRequestDto>)
        (item: RestRecordSaveRequestDto)
        =
        let update (updater: RestRecordSaveRequestDto -> RestRecordSaveRequestDto) =
            let index = items |> Seq.findIndex (fun rp -> rp.Id = item.Id)
            items[index] <- updater items[index]

        let handleDelete (id: Guid) =
            match items |> Seq.tryFind (fun rp -> rp.Id = id) with
            | Some rp -> items.Remove rp |> ignore
            | None -> ()

        StackPanel()
            .OrientationHorizontal()
            .Spacing(15.0)
            .Margin(0.0, 0.0, 0.0, 10.0)
            .Children(
                TimePickerField.create
                    { Label = "開始時間" |> R3.ret
                      BaseDate = Some ctx.CurrentDate
                      Value = Some item.StartedAt |> R3.ret
                      OnSetValue =
                        fun v ->
                            update (fun rp ->
                                { rp with
                                    StartedAt = defaultArg v rp.StartedAt })
                      IsClearable = false |> R3.ret
                      Errors =
                        ctx.FormCtx.Errors |> R3.map (WorkRecordErrors.chooseRestStartedAt item.Id) },
                TimePickerField.create
                    { Label = "終了時間" |> R3.ret
                      BaseDate = Some ctx.CurrentDate
                      Value = item.EndedAt |> R3.ret
                      OnSetValue = fun v -> update (fun rp -> { rp with EndedAt = v })
                      IsClearable = true |> R3.ret
                      Errors =
                        ctx.FormCtx.Errors |> R3.map (WorkRecordErrors.chooseRestEndedAt item.Id) },
                ComboBox()
                    .Width(100.0)
                    .ItemsSource([ RestVariantEnum.RegularRest; RestVariantEnum.PaidRest ])
                    .SelectedItem(item.Variant)
                    .OnSelectionChangedHandler(fun ctl _ ->
                        match ctl.SelectedItem with
                        | :? RestVariantEnum as v -> update (fun rp -> { rp with Variant = v })
                        | _ -> ())
                    .ItemTemplateFunc(fun (variant: RestVariantEnum) ->
                        TextBlock()
                            .Text(
                                match variant with
                                | RestVariantEnum.RegularRest -> "休憩"
                                | RestVariantEnum.PaidRest -> "有給休暇"
                                | _ -> "不明"
                            ))
                    .VerticalAlignmentBottom(),
                SymbolIconButton.create
                    { Symbol = Symbol.Delete |> R3.ret
                      OnClick = fun _ -> handleDelete item.Id
                      FontSize = Some 18.0 |> R3.ret
                      Tooltip = Some "この記録を削除する" |> R3.ret }
                |> _.VerticalAlignmentBottom()
            )

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<HistoryPageContext> self

            let restItems = ObservableList<RestRecordSaveRequestDto>()

            ctx.FormCtx.OnReset
            |> R3.subscribe (fun form ->
                restItems.Clear()
                restItems.AddRange form.RestRecords)
            |> disposables.Add

            // Sync from restItems to ctx.Form
            restItems
            |> R3.collectionChanged
            |> R3.subscribe (fun _ ->
                ctx.FormCtx.Form.Value <-
                    { ctx.FormCtx.Form.Value with
                        RestRecords = restItems |> Seq.toList })
            |> disposables.Add

            let addCommand =
                R3.command ()
                |> R3.withSubscribe disposables (fun () ->
                    RestRecordSaveRequestDto.empty ctx.CurrentDate.CurrentValue |> restItems.Add)

            let buildContent () =
                let isEmpty = ctx.FormCtx.Form |> R3.map _.RestRecords.IsEmpty

                Panel()
                    .Children(
                        TextBlock()
                            .Text("休憩・有給休暇の記録がありません。")
                            .FontSize(14.0)
                            .Foreground(Brushes.Gray)
                            .IsVisible(isEmpty |> asBinding),
                        ItemsControl()
                            .ItemsSourceObservable(restItems)
                            .ItemsPanelFunc(fun () -> VirtualizingStackPanel())
                            .TemplateFunc(fun () ->
                                let sv =
                                    ScrollViewer()
                                        .VerticalScrollBarVisibilityAuto()
                                        .HorizontalScrollBarVisibilityDisabled()
                                        .Content(ItemsPresenter())

                                ctx.CurrentDate
                                |> R3.distinctUntilChanged
                                |> R3.subscribe (fun _ -> sv.ScrollToHome())
                                |> disposables.Add

                                addCommand
                                |> R3.subscribe (fun _ -> nextTick (fun () -> sv.ScrollToEnd()))
                                |> disposables.Add

                                sv)
                            .ItemTemplateFunc(createRestItemView ctx restItems)
                            .IsVisible(isEmpty |> R3.map not |> asBinding)
                    )

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
                                        .Text("休憩・有給休暇")
                                        .FontSize(18.0)
                                        .FontWeightBold()
                                        .Column(0),
                                    SymbolIconButton.create
                                        { Symbol = Symbol.Add |> R3.ret
                                          OnClick = fun _ -> addCommand.Execute()
                                          FontSize = Some 20.0 |> R3.ret
                                          Tooltip = Some "休憩・有給休暇を追加" |> R3.ret }
                                    |> _.Column(2)
                                )
                                .Row(0),
                            buildContent () |> _.Row(1)
                        )
                ))
