namespace AttendanceRecord.Presentation.Views.HistoryPage

open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common.Navigation
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

            // Register navigation guard
            match Context.resolve<NavigationContext> self with
            | Some(navCtx, _) -> navCtx.RegisterGuard ctx.ConfirmDiscard |> disposables.Add
            | None -> ()

            let showEditView = ctx.CurrentDate |> R3.map Option.isSome

            DockPanel()
                .LastChildFill(true)
                .Children(
                    HistoryToolbar.create () |> _.DockTop(),
                    Grid()
                        .ColumnDefinitions("250,5,*")
                        .Children(
                            WorkRecordListView.create () |> _.Column(0),
                            GridSplitter()
                                .Column(1)
                                .Width(1.0)
                                .Background(Avalonia.Media.Brushes.DimGray)
                                .ResizeDirectionColumns(),
                            WorkRecordEditView.create ()
                            |> _.Column(2).IsVisible(showEditView |> asBinding),
                            TextBlock()
                                .Text("日付を選択してください。")
                                .FontSize(16.0)
                                .HorizontalAlignmentCenter()
                                .VerticalAlignmentCenter()
                            |> _.Column(2).IsVisible(showEditView |> R3.map not |> asBinding)
                        )
                )
            |> Context.provide ctx)
