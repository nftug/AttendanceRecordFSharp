namespace AttendanceRecord.Presentation.Views.HistoryPage

open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common.Navigation
open AttendanceRecord.Presentation.Views.Common.Context
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Presentation.Views.HistoryPage.Edit
open AttendanceRecord.Presentation.Views.HistoryPage.Navigation
open AttendanceRecord.Shared

module HistoryPageView =
   open NXUI.Extensions
   open type NXUI.Builders

   let create (props: HistoryPageContextProps) : Avalonia.Controls.Control =
      withLifecycle (fun disposables self ->
         let ctx = createHistoryPageContext props disposables
         let themeCtx = Context.require<ThemeContext> self |> fst

         // Register navigation guard
         Context.resolve<NavigationContext> self
         |> Option.iter (fun (navCtx, _) ->
            navCtx.RegisterGuard ctx.ConfirmDiscard |> disposables.Add)

         let toShowEditView = ctx.SelectedDate |> R3.map Option.isSome

         DockPanel()
            .LastChildFill(true)
            .Children(
               HistoryToolbar.create () |> _.DockTop(),
               Grid()
                  .ColumnDefinitions("250,5,*")
                  .Children(
                     WorkRecordListView.create () |> _.Margin(5.0, 5.0, 0.0, 5.0).Column(0),
                     GridSplitter()
                        .Column(1)
                        .Width(1.0)
                        .Margin(0.0, 5.0)
                        .Background(
                           themeCtx.GetBrushResourceObservable "DividerStrokeColorDefaultBrush"
                           |> asBinding
                        )
                        .ResizeDirectionColumns(),
                     WorkRecordEditView.create ()
                     |> _.Column(2).IsVisible(toShowEditView |> asBinding),
                     TextBlock()
                        .Text("日付を選択してください。")
                        .FontSize(16.0)
                        .HorizontalAlignmentCenter()
                        .VerticalAlignmentCenter()
                     |> _.Column(2).IsVisible(toShowEditView |> R3.map not |> asBinding)
                  )
            )
         |> Context.provide ctx)
