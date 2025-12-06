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

         let expander =
            SettingsExpander(
               Header = "休憩開始前のアラーム",
               Description = "休憩開始前にアラームを表示する設定を行います。",
               IconSource = SymbolIconSource(Symbol = enum<Symbol> 0xEC32)
            )

         expander.Items.Add(
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
         )
         |> ignore

         expander.Items.Add(
            let footer =
               StackPanel()
                  .Spacing(15.0)
                  .Children(
                     StackPanel()
                        .Spacing(12.0)
                        .OrientationHorizontal()
                        .Children(
                           NumericUpDown()
                              .Value(beforeStartMinutes |> asBinding)
                              .OnValueChangedHandler(fun _ e ->
                                 update (fun f ->
                                    { f with
                                       BeforeStartMinutes = e.NewValue |> decimal |> float }))
                              .FormatString("0")
                              .Minimum(0m)
                              .Maximum(1440.0m)
                              .Width(120.0)
                              .IsEnabled(alarmEnabled |> asBinding),
                           TextBlock().Text("分後").VerticalAlignmentCenter()
                        )
                        .HorizontalAlignmentRight(),
                     ValidationErrorsText.create
                        { Errors =
                           ctx.FormCtx.Errors
                           |> R3.map AppConfigErrors.chooseRestStartAlarmDuration
                          FontSize = None }
                  )

            SettingsExpanderItem(
               Content = "休憩開始前の経過時間",
               Description = "休憩開始前にアラームを表示する時間を分単位で設定します。",
               Footer = footer
            )
         )
         |> ignore

         expander.Items.Add(
            let footer =
               StackPanel()
                  .Spacing(15.0)
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
                              .Maximum(60m)
                              .Width(120.0)
                              .IsEnabled(alarmEnabled |> asBinding),
                           TextBlock().Text("分間").VerticalAlignmentCenter()
                        )
                        .HorizontalAlignmentRight(),
                     ValidationErrorsText.create
                        { Errors =
                           ctx.FormCtx.Errors
                           |> R3.map AppConfigErrors.chooseRestStartAlarmSnoozeDuration
                          FontSize = None }
                  )

            SettingsExpanderItem(
               Content = "スヌーズ",
               Description = "アラームを再度表示するまでの時間を分単位で設定します。",
               Footer = footer
            )
         )
         |> ignore

         expander)
