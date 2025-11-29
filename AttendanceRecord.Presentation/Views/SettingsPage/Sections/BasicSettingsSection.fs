namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open System
open R3
open Avalonia.Media
open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Domain.Errors

module BasicSettingsSection =
    let create () =
        withLifecycle (fun _ self ->
            let ctx, _ = Context.require<SettingsPageContext> self

            let updateStandardWorkMinutes (value: Nullable<decimal>) =
                ctx.FormCtx.Form.Value <-
                    { ctx.FormCtx.Form.Value with
                        StandardWorkTimeMinutes = value |> decimal |> float }

                ctx.FormCtx.Errors.Value <-
                    ctx.FormCtx.Errors.Value |> List.filter (_.IsStandardWorkTimeError >> not)

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(20.0)
                .Child(
                    StackPanel()
                        .Spacing(15.0)
                        .Children(
                            TextBlock().Text("基本設定").FontSize(18.0).FontWeightBold(),
                            StackPanel()
                                .OrientationHorizontal()
                                .Spacing(10.0)
                                .Children(
                                    TextBlock()
                                        .Text("標準勤務時間 (分)")
                                        .VerticalAlignmentCenter()
                                        .Width(150.0),
                                    NumericUpDown()
                                        .Value(
                                            ctx.FormCtx.Form
                                            |> R3.map (_.StandardWorkTimeMinutes >> decimal)
                                            |> asBinding
                                        )
                                        .OnValueChangedHandler(fun _ e ->
                                            updateStandardWorkMinutes e.NewValue)
                                        .FormatString("0")
                                        .Minimum(0m)
                                        .Maximum(1440m)
                                        .Increment(15m)
                                        .Width(120.0)
                                        .Errors(
                                            ctx.FormCtx.Errors
                                            |> R3.map AppConfigErrors.chooseStandardWorkTime
                                            |> asBinding
                                        )
                                ),
                            TextBlock()
                                .Text("1日の標準勤務時間を分単位で指定します。")
                                .FontSize(12.0)
                                .Foreground(Brushes.Gray)
                        )
                ))
