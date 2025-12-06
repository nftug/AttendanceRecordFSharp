namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open NXUI.Extensions
open type NXUI.Builders
open FluentAvalonia.UI.Controls
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Application.Dtos.Requests

module WorkEndAlarmSection =
   let create () =
      withLifecycle (fun _ self ->
         let ctx, _ = Context.require<SettingsPageContext> self
         let workEndAlarm = ctx.FormCtx.Form |> R3.map _.WorkEndAlarm
         let alarmEnabled = workEndAlarm |> R3.map _.IsEnabled
         let beforeEndMinutes = workEndAlarm |> R3.map (_.BeforeEndMinutes >> decimal)
         let snoozeMinutes = workEndAlarm |> R3.map (_.SnoozeMinutes >> decimal)

         let update
            (updater: WorkEndAlarmConfigSaveRequestDto -> WorkEndAlarmConfigSaveRequestDto)
            =
            ctx.FormCtx.Form.Value <-
               { ctx.FormCtx.Form.Value with
                  WorkEndAlarm = updater ctx.FormCtx.Form.Value.WorkEndAlarm }

            ctx.FormCtx.Errors.Value <-
               ctx.FormCtx.Errors.Value |> List.filter (_.IsWorkEndAlarmError >> not)

         let toggleSubsection =
            let footer =
               ToggleSwitch()
                  .OnContent("有効")
                  .OffContent("無効")
                  .IsChecked(alarmEnabled |> asBinding)
                  .OnIsCheckedChangedHandler(fun ctl _ ->
                     update (fun f ->
                        { f with
                           IsEnabled = ctl.IsChecked.GetValueOrDefault false }))

            SettingsExpanderItem(Content = "アラームを有効にする", Footer = footer)

         let durationSubsection =
            let footer =
               StackPanel()
                  .Spacing(5.0)
                  .Children(
                     StackPanel()
                        .Spacing(12.0)
                        .OrientationHorizontal()
                        .Children(
                           NumericUpDown()
                              .Value(beforeEndMinutes |> asBinding)
                              .OnValueChangedHandler(fun _ e ->
                                 update (fun f ->
                                    { f with
                                       BeforeEndMinutes = e.NewValue |> decimal |> float }))
                              .FormatString("0")
                              .Minimum(0m)
                              .Maximum(1440.0m)
                              .Width(120.0)
                              .IsEnabled(alarmEnabled |> asBinding),
                           TextBlock().Text("分前").VerticalAlignmentCenter()
                        )
                        .HorizontalAlignmentRight(),
                     ValidationErrorsText.create (
                        ctx.FormCtx.Errors |> R3.map AppConfigErrors.chooseWorkEndAlarmDuration
                     )
                  )

            SettingsExpanderItem(
               Content = "勤務終了前の残り時間",
               Description = "勤務終了前にアラームを表示する時間を分単位で設定します。",
               Footer = footer
            )

         let snoozeDurationSubsection =
            let footer =
               StackPanel()
                  .Spacing(5.0)
                  .Children(
                     StackPanel()
                        .Spacing(12.0)
                        .OrientationHorizontal()
                        .Children(
                           NumericUpDown()
                              .Value(snoozeMinutes |> asBinding)
                              .OnValueChangedHandler(fun _ e ->
                                 update (fun f ->
                                    { f with
                                       SnoozeMinutes = e.NewValue |> decimal |> float }))
                              .FormatString("0")
                              .Minimum(1m)
                              .Maximum(60.0m)
                              .Width(120.0)
                              .IsEnabled(alarmEnabled |> asBinding),
                           TextBlock().Text("分間").VerticalAlignmentCenter()
                        )
                        .HorizontalAlignmentRight(),
                     ValidationErrorsText.create (
                        ctx.FormCtx.Errors
                        |> R3.map AppConfigErrors.chooseWorkEndAlarmSnoozeDuration
                     )
                  )

            SettingsExpanderItem(
               Content = "スヌーズ",
               Description = "アラームを再度表示するまでの時間を分単位で設定します。",
               Footer = footer
            )

         SettingsExpander(
            Header = "勤務終了前のアラーム",
            Description = "勤務終了前にアラームを表示する設定を行います。",
            IconSource = SymbolIconSource(Symbol = Symbol.Alert),
            ItemsSource = [ toggleSubsection; durationSubsection; snoozeDurationSubsection ]
         ))
