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
   open Avalonia.Layout

   let private createRestItemView
      (ctx: HistoryPageContext)
      (items: ObservableList<RestRecordSaveRequestDto>)
      (item: RestRecordSaveRequestDto)
      =
      let update (updater: RestRecordSaveRequestDto -> RestRecordSaveRequestDto) =
         let index = items |> Seq.findIndex (fun rp -> rp.Id = item.Id)
         items[index] <- updater items[index]

         ctx.FormCtx.Errors.Value <-
            ctx.FormCtx.Errors.Value
            |> List.filter (function
               | WorkRestsErrors restErrors ->
                  RestRecordErrors.chooseAll restErrors
                  |> List.exists (fun (errId, _) -> errId <> item.Id)
               | _ -> true)

      let handleDelete (id: Guid) =
         items
         |> Seq.tryFind (fun rp -> rp.Id = id)
         |> Option.iter (items.Remove >> ignore)

      let errors =
         ctx.FormCtx.Errors
         |> R3.map (fun errors ->
            WorkRecordErrors.chooseRestsAll errors
            |> List.filter (fun (errId, _) -> errId = item.Id)
            |> List.map snd)

      let handleRestVariantChange (variant: RestVariantEnum) =
         update (fun rp ->
            // If added new paid rest without endedAt, set endedAt to current date
            let endedAt =
               match variant with
               | RestVariantEnum.PaidRest when rp.EndedAt.IsNone ->
                  Some ctx.CurrentDate.CurrentValue.Date
               | _ -> rp.EndedAt

            { rp with
               Variant = variant
               EndedAt = endedAt })

      StackPanel()
         .OrientationHorizontal()
         .Spacing(15.0)
         .Margin(0.0, 0.0, 0.0, 10.0)
         .Children(
            TimeDurationPicker.create
               { StartedAt = item.StartedAt |> R3.ret
                 EndedAt = item.EndedAt |> R3.ret
                 OnStartedAtChanged =
                  fun v ->
                     update (fun rp ->
                        { rp with
                           StartedAt = defaultArg v rp.StartedAt })
                 OnEndedAtChanged = fun v -> update (fun rp -> { rp with EndedAt = v })
                 Errors = errors
                 Spacing = Some 15.0
                 IsEndedAtClearable = item.Variant <> RestVariantEnum.PaidRest |> R3.ret },
            StackPanel()
               .OrientationHorizontal()
               .VerticalAlignment(
                  errors
                  |> R3.map (function
                     | [] -> VerticalAlignment.Bottom
                     | _ -> VerticalAlignment.Center)
                  |> asBinding
               )
               .Spacing(10.0)
               .Children(
                  ComboBox()
                     .Width(100.0)
                     .ItemsSource(RestVariantEnum.all)
                     .SelectedItem(item.Variant)
                     .OnSelectionChangedHandler(fun ctl _ ->
                        match ctl.SelectedItem with
                        | :? RestVariantEnum as v -> handleRestVariantChange v
                        | _ -> ())
                     .ItemTemplateFunc(fun (variant: RestVariantEnum) ->
                        TextBlock().Text(RestVariantEnum.toDisplayString variant)),
                  SymbolIconButton.create
                     { Symbol = Symbol.Delete |> R3.ret
                       OnClick = fun _ -> handleDelete item.Id
                       FontSize = Some 18.0 |> R3.ret
                       Tooltip = Some "この記録を削除する" |> R3.ret }
               )
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
            R3.command<RestVariantEnum> ()
            |> R3.withSubscribe disposables (fun variant ->
               RestRecordSaveRequestDto.empty variant ctx.CurrentDate.CurrentValue
               |> restItems.Add)

         let buildAddSplitButton () =
            DropDownButton()
               .Content(SymbolIcon.create (Symbol.Add |> R3.ret))
               .FontSize(20.0)
               .Tip("休憩・有給休暇を追加する")
               .Flyout(
                  MenuFlyout()
                     .ItemsSource(
                        RestVariantEnum.all
                        |> List.map (fun variant ->
                           MenuItem()
                              .Header($"{RestVariantEnum.toDisplayString variant}を追加")
                              .OnClickHandler(fun _ _ -> addCommand.Execute variant))
                     )
               )

         let buildContent () =
            let isEmpty = ctx.FormCtx.Form |> R3.map _.RestRecords.IsEmpty

            let restItemsNotification =
               restItems.ToNotifyCollectionChangedSlim() |> R3.disposeWith disposables

            Panel()
               .Children(
                  TextBlock()
                     .Text("休憩・有給休暇の記録がありません。")
                     .FontSize(14.0)
                     .Foreground(Brushes.Gray)
                     .IsVisible(isEmpty |> asBinding),
                  ItemsControl()
                     .ItemsSourceNotification(restItemsNotification)
                     .ItemsPanelFunc(fun () -> VirtualizingStackPanel())
                     .TemplateFunc(fun () ->
                        let sv = ScrollViewer().Content(ItemsPresenter())

                        ctx.CurrentDate
                        |> R3.distinctUntilChanged
                        |> R3.subscribe (fun _ -> sv.ScrollToHome())
                        |> disposables.Add

                        addCommand
                        |> R3.subscribe (fun _ -> nextTick sv.ScrollToEnd)
                        |> disposables.Add

                        sv)
                     .ItemTemplateFunc(createRestItemView ctx restItems)
                     .IsVisible(isEmpty |> R3.map not |> asBinding)
               )

         (ControlBorder.create ())
            .Padding(15.0)
            .Child(
               Grid()
                  .RowDefinitions("Auto,*")
                  .RowSpacing(15.0)
                  .Children(
                     Grid()
                        .ColumnDefinitions("Auto,*,Auto")
                        .Children(
                           TextBlock().Text("休憩・有給休暇").FontSize(18.0).Column(0),
                           buildAddSplitButton () |> _.Column(2)
                        )
                        .Row(0),
                     buildContent () |> _.Row(1)
                  )
            ))
