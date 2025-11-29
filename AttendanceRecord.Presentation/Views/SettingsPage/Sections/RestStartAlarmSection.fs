namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open Avalonia.Media
open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Application.Dtos.Requests

module RestStartAlarmSection =
    let create () =
        withLifecycle (fun _ self ->
            let ctx, _ = Context.require<SettingsPageContext> self
            let restStartAlarm = ctx.FormCtx.Form |> R3.map _.RestStartAlarm
            let alarmEnabled = restStartAlarm |> R3.map _.IsEnabled
            let beforeStartMinutes = restStartAlarm |> R3.map (_.BeforeStartMinutes >> decimal)
            let snoozeMinutes = restStartAlarm |> R3.map (_.SnoozeMinutes >> decimal)

            let update
                (updater: RestStartAlarmConfigSaveRequestDto -> RestStartAlarmConfigSaveRequestDto)
                =
                ctx.FormCtx.Form.Value <-
                    { ctx.FormCtx.Form.Value with
                        RestStartAlarm = updater ctx.FormCtx.Form.Value.RestStartAlarm }

                ctx.FormCtx.Errors.Value <-
                    ctx.FormCtx.Errors.Value |> List.filter (_.IsRestStartAlarmError >> not)

            let buildDurationPart () =
                StackPanel()
                    .Spacing(15.0)
                    .Children(
                        StackPanel()
                            .OrientationHorizontal()
                            .Spacing(10.0)
                            .Children(
                                TextBlock().Text("経過時間 (分)").VerticalAlignmentCenter().Width(120.0),
                                NumericUpDown()
                                    .Value(beforeStartMinutes |> asBinding)
                                    .OnValueChangedHandler(fun _ e ->
                                        update (fun f ->
                                            { f with
                                                BeforeStartMinutes =
                                                    e.NewValue |> decimal |> float }))
                                    .FormatString("0")
                                    .Minimum(0m)
                                    .Maximum(1440.0m)
                                    .Width(120.0)
                                    .IsEnabled(alarmEnabled |> asBinding)
                            ),
                        ValidationErrorsText.create
                            { Errors =
                                ctx.FormCtx.Errors
                                |> R3.map AppConfigErrors.chooseRestStartAlarmDuration
                              FontSize = None }
                    )

            let buildSnoozePart () =
                StackPanel()
                    .Spacing(15.0)
                    .Children(
                        StackPanel()
                            .OrientationHorizontal()
                            .Spacing(10.0)
                            .Children(
                                TextBlock().Text("スヌーズ (分)").VerticalAlignmentCenter().Width(120.0),
                                NumericUpDown()
                                    .Value(snoozeMinutes |> asBinding)
                                    .OnValueChangedHandler(fun _ e ->
                                        update (fun f ->
                                            { f with
                                                SnoozeMinutes = e.NewValue |> decimal |> float }))
                                    .FormatString("0")
                                    .Minimum(1m)
                                    .Maximum(60m)
                                    .Width(120.0)
                                    .IsEnabled(alarmEnabled |> asBinding)
                            ),
                        ValidationErrorsText.create
                            { Errors =
                                ctx.FormCtx.Errors
                                |> R3.map AppConfigErrors.chooseRestStartAlarmSnoozeDuration
                              FontSize = None }
                    )

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(20.0)
                .Child(
                    StackPanel()
                        .Spacing(15.0)
                        .Children(
                            TextBlock().Text("休憩開始アラーム").FontSize(18.0).FontWeightBold(),
                            ToggleSwitch()
                                .Content("アラームを有効にする")
                                .IsChecked(alarmEnabled |> asBinding)
                                .OnIsCheckedChangedHandler(fun ctl _ ->
                                    update (fun f ->
                                        { f with
                                            IsEnabled = ctl.IsChecked.GetValueOrDefault false })),
                            StackPanel()
                                .OrientationHorizontal()
                                .Spacing(30.0)
                                .Children(buildDurationPart (), buildSnoozePart ())
                        )
                ))
