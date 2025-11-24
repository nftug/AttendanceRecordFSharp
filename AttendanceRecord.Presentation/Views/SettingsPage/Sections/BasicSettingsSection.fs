namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open R3
open Avalonia.Media
open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Requests

module BasicSettingsSection =
    let create () =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<SettingsPageContext> self
            let standardWorkMinutes = R3.property 0m |> R3.disposeWith disposables

            ctx.FormCtx.OnReset
            |> R3.subscribe (fun form ->
                standardWorkMinutes.Value <- decimal form.StandardWorkTimeMinutes)
            |> disposables.Add

            standardWorkMinutes
            |> R3.distinctUntilChanged
            |> R3.subscribe (fun minutes ->
                ctx.FormCtx.Form.Value <-
                    { ctx.FormCtx.Form.Value with
                        StandardWorkTimeMinutes = float minutes })
            |> disposables.Add

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
                                        .Value(standardWorkMinutes |> asBinding)
                                        .OnValueChangedHandler(fun _ e ->
                                            standardWorkMinutes.Value <- e.NewValue |> decimal)
                                        .FormatString("0")
                                        .Minimum(0m)
                                        .Maximum(1440m)
                                        .Increment(15m)
                                        .Width(120.0)
                                ),
                            TextBlock()
                                .Text("1日の標準勤務時間を分単位で指定します。")
                                .FontSize(12.0)
                                .Foreground(Brushes.Gray)
                        )
                ))
