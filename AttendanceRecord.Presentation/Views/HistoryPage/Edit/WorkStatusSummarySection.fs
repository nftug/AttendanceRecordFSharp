namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open System
open R3
open Avalonia.Media
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Responses

module WorkStatusSummarySection =
    open NXUI.Extensions
    open type NXUI.Builders

    let private createSummaryInfoRow (label: string) (duration: Observable<TimeSpan>) =
        let durationText = duration |> R3.map TimeSpan.formatDuration

        StackPanel()
            .OrientationHorizontal()
            .Spacing(3.0)
            .Children(
                TextBlock().Text(label).FontWeightBold().Width(120.0),
                TextBlock().Text(durationText |> asBinding).VerticalAlignmentCenter()
            )

    let create () =
        withLifecycle (fun _ self ->
            let ctx, _ = Context.require<HistoryPageContext> self

            let summary =
                ctx.CurrentSummary |> R3.map (Option.defaultValue WorkRecordSummaryDto.empty)

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(18.0)
                .Child(
                    StackPanel()
                        .Spacing(10.0)
                        .Children(
                            createSummaryInfoRow "勤務時間" (summary |> R3.map _.TotalWorkTime),
                            createSummaryInfoRow "休憩時間" (summary |> R3.map _.TotalRestTime),
                            createSummaryInfoRow "残業時間" (summary |> R3.map _.Overtime)
                        )
                ))
