namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open Avalonia.Media
open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared

module RestStartAlarmSection =
    let create () =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<SettingsPageContext> self
            let alarmEnabled = R3.property false |> R3.disposeWith disposables
            let beforeStartMinutes = R3.property 0m |> R3.disposeWith disposables
            let snoozeMinutes = R3.property 0m |> R3.disposeWith disposables
            let maxMinutes = ctx.Form |> R3.map _.StandardWorkTimeMinutes |> R3.map decimal

            ctx.ResetCommand
            |> R3.prepend ctx.DefaultForm.CurrentValue
            |> R3.map _.RestStartAlarmConfig
            |> R3.subscribe (fun config ->
                alarmEnabled.Value <- config.IsEnabled
                beforeStartMinutes.Value <- decimal config.BeforeStartDurationMinutes
                snoozeMinutes.Value <- decimal config.SnoozeDurationMinutes)
            |> disposables.Add

            R3.combineLatest3 alarmEnabled beforeStartMinutes snoozeMinutes
            |> R3.distinctUntilChanged
            |> R3.subscribe (fun (isEnabled, beforeMinutes, snoozeMinutes) ->
                ctx.Form.Value <-
                    { ctx.Form.Value with
                        RestStartAlarmConfig.IsEnabled = isEnabled
                        RestStartAlarmConfig.BeforeStartDurationMinutes = float beforeMinutes
                        RestStartAlarmConfig.SnoozeDurationMinutes = float snoozeMinutes })
            |> disposables.Add

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(20.0)
                .Child(
                    StackPanel()
                        .Spacing(15.0)
                        .Children(
                            TextBlock().Text("休憩開始アラーム").FontSize(18.0).FontWeightBold(),
                            StackPanel()
                                .OrientationHorizontal()
                                .Spacing(10.0)
                                .Children(
                                    ToggleSwitch()
                                        .Content("アラームを有効にする")
                                        .IsChecked(alarmEnabled |> asBinding)
                                        .OnIsCheckedChangedHandler(fun ctl _ ->
                                            alarmEnabled.Value <-
                                                ctl.IsChecked.GetValueOrDefault false)
                                ),
                            StackPanel()
                                .OrientationHorizontal()
                                .Spacing(30.0)
                                .Children(
                                    StackPanel()
                                        .OrientationHorizontal()
                                        .Spacing(10.0)
                                        .Children(
                                            TextBlock()
                                                .Text("経過時間 (分)")
                                                .VerticalAlignmentCenter()
                                                .Width(120.0),
                                            NumericUpDown()
                                                .Value(beforeStartMinutes |> asBinding)
                                                .OnValueChangedHandler(fun _ e ->
                                                    beforeStartMinutes.Value <-
                                                        e.NewValue |> decimal)
                                                .FormatString("0")
                                                .Minimum(0m)
                                                .Maximum(maxMinutes |> asBinding)
                                                .Width(120.0)
                                                .IsEnabled(alarmEnabled |> asBinding)
                                        ),
                                    StackPanel()
                                        .OrientationHorizontal()
                                        .Spacing(10.0)
                                        .Children(
                                            TextBlock()
                                                .Text("スヌーズ (分)")
                                                .VerticalAlignmentCenter()
                                                .Width(120.0),
                                            NumericUpDown()
                                                .Value(snoozeMinutes |> asBinding)
                                                .OnValueChangedHandler(fun _ e ->
                                                    snoozeMinutes.Value <- e.NewValue |> decimal)
                                                .FormatString("0")
                                                .Minimum(1m)
                                                .Maximum(60m)
                                                .Width(120.0)
                                                .IsEnabled(alarmEnabled |> asBinding)
                                        )
                                )
                        )
                ))
