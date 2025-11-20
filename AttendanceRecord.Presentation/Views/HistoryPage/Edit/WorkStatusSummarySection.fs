namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open Avalonia.Media
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.HistoryPage.Context

module WorkStatusSummarySection =
    open NXUI.Extensions
    open type NXUI.Builders
    open AttendanceRecord.Shared

    let private createSummaryInfoRow (label: string) (value: string) =
        StackPanel()
            .OrientationHorizontal()
            .Children(
                Label().Content(label).FontSize(14.0).FontWeightBold().Width(120.0),
                Label().Content(value).FontSize(14.0).VerticalAlignmentCenter()
            )

    let createSummarySection () =
        withReactive (fun _ self ->
            let ctx, _ = HistoryPageContextProvider.require self

            ctx.CurrentStatus
            |> toView (fun statusOpt ->
                match statusOpt with
                | None -> Panel()
                | Some status ->
                    Border()
                        .BorderThickness(1.0)
                        .BorderBrush(Brushes.Gray)
                        .Padding(15.0)
                        .Child(
                            StackPanel()
                                .Spacing(10.0)
                                .Children(
                                    TextBlock().Text("勤務記録の概要").FontSize(18.0).FontWeightBold(),
                                    createSummaryInfoRow
                                        "勤務時間"
                                        (TimeSpan.formatDuration status.WorkTimeDuration),
                                    createSummaryInfoRow
                                        "休憩時間"
                                        (TimeSpan.formatDuration status.RestTimeDuration),
                                    createSummaryInfoRow
                                        "残業時間"
                                        (TimeSpan.formatDuration status.OvertimeDuration)
                                )
                        )))
