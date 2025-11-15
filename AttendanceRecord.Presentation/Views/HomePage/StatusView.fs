namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open R3
open Material.Icons
open Avalonia.Media
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Application.Dtos.Responses

type private StatusInfo = { Label: string; Value: string }

module StatusView =
    let private getStatusInfo status =
        [ { Label = "勤務時間"
            Value = status.WorkDuration.ToString @"hh\:mm\:ss" }
          { Label = "休憩時間"
            Value = status.RestDuration.ToString @"hh\:mm\:ss" }
          { Label = "本日の残業時間"
            Value = status.OvertimeDuration.ToString @"hh\:mm\:ss" }
          { Label = "今月の残業時間"
            Value = status.OvertimeMonthlyDuration.ToString @"hh\:mm\:ss" } ]

    let create () : Avalonia.Controls.Control =
        withReactive (fun _ self ->
            let ctx, _ = HomePageContextProvider.require self

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(25.0)
                .Height(250.0)
                .Child(
                    ctx.Status
                    |> R3.map getStatusInfo
                    |> toView (fun rows ->
                        StackPanel()
                            .Spacing(8.0)
                            .VerticalAlignmentCenter()
                            .Children(
                                rows
                                |> List.map (fun row ->
                                    TextBlock().Text($"{row.Label}: {row.Value}").FontSize(16.0))
                                |> toChildren
                            ))
                ))
