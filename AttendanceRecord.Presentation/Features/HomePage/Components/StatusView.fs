namespace AttendanceRecord.Presentation.Features.HomePage.Components

open Avalonia
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open AttendanceRecord.Application.Dtos.Responses

type StatusViewProps = { Status: IReadable<CurrentStatusDto> }

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

    let view (props: StatusViewProps) =
        Component.create (
            "StatusView",
            fun ctx ->
                let statusInfo = ctx.useDerived1 (props.Status, getStatusInfo)

                Border.create
                    [ Border.borderThickness (Thickness 1.0)
                      Border.borderBrush "Gray"
                      Border.padding 25.0
                      Border.height 250.0
                      Border.child (
                          StackPanel.create
                              [ StackPanel.spacing 8.0
                                StackPanel.verticalAlignment VerticalAlignment.Center
                                StackPanel.children (
                                    statusInfo.Current
                                    |> List.map (fun item ->
                                        CjkTextBlock.create
                                            [ TextBlock.text $"{item.Label}: {item.Value}"
                                              TextBlock.fontSize 16.0
                                              TextBlock.horizontalAlignment HorizontalAlignment.Left ])
                                ) ]
                      ) ]
        )
