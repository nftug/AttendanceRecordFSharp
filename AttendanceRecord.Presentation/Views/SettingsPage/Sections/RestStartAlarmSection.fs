namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open Avalonia.Media
open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared
open AttendanceRecord.Domain.Errors

module RestStartAlarmSection =
    let create () =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<SettingsPageContext> self
            let alarmEnabled = R3.property false |> R3.disposeWith disposables
            let beforeStartMinutes = R3.property 0m |> R3.disposeWith disposables
            let snoozeMinutes = R3.property 0m |> R3.disposeWith disposables

            let maxMinutes =
                ctx.FormCtx.Form |> R3.map _.StandardWorkTimeMinutes |> R3.map decimal

            ctx.FormCtx.OnReset
            |> R3.map _.RestStartAlarm
            |> R3.subscribe (fun config ->
                alarmEnabled.Value <- config.IsEnabled
                beforeStartMinutes.Value <- decimal config.BeforeStartMinutes
                snoozeMinutes.Value <- decimal config.SnoozeMinutes)
            |> disposables.Add

            R3.combineLatest3 alarmEnabled beforeStartMinutes snoozeMinutes
            |> R3.distinctUntilChanged
            |> R3.subscribe (fun (isEnabled, beforeMinutes, snoozeMinutes) ->
                ctx.FormCtx.Form.Value <-
                    { ctx.FormCtx.Form.Value with
                        RestStartAlarm.IsEnabled = isEnabled
                        RestStartAlarm.BeforeStartMinutes = float beforeMinutes
                        RestStartAlarm.SnoozeMinutes = float snoozeMinutes })
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
                                                .Errors(
                                                    ctx.FormCtx.Errors
                                                    |> R3.map
                                                        AppConfigErrors.chooseRestStartAlarmDuration
                                                    |> asBinding
                                                )
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
                                                .Errors(
                                                    ctx.FormCtx.Errors
                                                    |> R3.map
                                                        AppConfigErrors.chooseRestStartAlarmSnoozeDuration
                                                    |> asBinding
                                                )
                                        )
                                )
                        )
                ))
