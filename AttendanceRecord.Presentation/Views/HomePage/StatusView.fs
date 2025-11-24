namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open R3
open System
open Material.Icons
open Avalonia.Media
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Presentation.Views.HomePage.Context

module StatusView =
    let private createSummaryInfoRow (label: string) (duration: Observable<TimeSpan>) =
        let durationText = duration |> R3.map TimeSpan.formatDuration

        StackPanel()
            .OrientationHorizontal()
            .Spacing(5.0)
            .Children(
                TextBlock().Text(label).FontWeightBold().FontSize(16.0).Width(150.0),
                TextBlock().Text(durationText |> asBinding).FontSize(16.0).VerticalAlignmentCenter()
            )

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun _ self ->
            let ctx, _ = Context.require<HomePageContext> self

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(25.0)
                .Height(250.0)
                .Child(
                    StackPanel()
                        .Spacing(8.0)
                        .VerticalAlignmentCenter()
                        .Children(
                            createSummaryInfoRow
                                "勤務時間"
                                (ctx.Status |> R3.map _.Summary.TotalWorkTime),
                            createSummaryInfoRow
                                "休憩時間"
                                (ctx.Status |> R3.map _.Summary.TotalRestTime),
                            createSummaryInfoRow
                                "今日の残業時間"
                                (ctx.Status |> R3.map _.Summary.Overtime),
                            createSummaryInfoRow "今月の残業時間" (ctx.Status |> R3.map _.OvertimeMonthly)
                        )
                ))
