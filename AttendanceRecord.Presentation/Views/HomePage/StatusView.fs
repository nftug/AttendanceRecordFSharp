namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open AttendanceRecord.Presentation.Utils
open R3
open Material.Icons
open Avalonia.Media
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Responses

type StatusViewProps =
    { Status: Observable<CurrentStatusDto> }

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

    let create (props: StatusViewProps) : Avalonia.Controls.Control =
        withReactive (fun disposables _ ->
            let container =
                Border()
                    .BorderThickness(1.0)
                    .BorderBrush(Brushes.Gray)
                    .Padding(25.0)
                    .Height(250.0)

            props.Status
            |> R3.map getStatusInfo
            |> R3.subscribe (fun rows ->
                container.Child <-
                    StackPanel()
                        .Spacing(8.0)
                        .VerticalAlignmentCenter()
                        .Children(
                            rows
                            |> List.map (fun row -> TextBlock().Text($"{row.Label}: {row.Value}").FontSize(16.0))
                            |> toChildren
                        ))
            |> disposables.Add

            container)
