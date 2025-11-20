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
            .Spacing(3.0)
            .Children(
                TextBlock().Text(label).FontWeightBold().Width(120.0),
                TextBlock().Text(value).VerticalAlignmentCenter()
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
                        .Padding(18.0)
                        .Child(
                            StackPanel()
                                .Spacing(10.0)
                                .Children(
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
