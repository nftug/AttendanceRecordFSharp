namespace AttendanceRecord.Presentation.Views.HistoryPage

open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Presentation.Views.HistoryPage.Edit
open AttendanceRecord.Presentation.Views.HistoryPage.Navigation

module HistoryPageView =
    open NXUI.Extensions
    open type NXUI.Builders

    let create (props: HistoryPageContextProps) : Avalonia.Controls.Control =
        withLifecycle (fun disposables _ ->
            useHistoryPageContext props disposables
            |> HistoryPageContextProvider.provide (
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
                                WorkRecordEditView.create
                                    { SaveWorkRecord = props.SaveWorkRecord
                                      DeleteWorkRecord = props.DeleteWorkRecord
                                      GetWorkRecordDetails = props.GetWorkRecordDetails }
                                |> _.Column(2)
                            )
                    )
            ))
