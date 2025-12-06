namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open System
open NXUI.Extensions
open type NXUI.Builders
open FluentAvalonia.UI.Controls
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
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

         let expander =
            SettingsExpander(
               Header = "基本設定",
               Description = "システムの基本的な動作に関する設定を行います。",
               IconSource = SymbolIconSource(Symbol = Symbol.Setting)
            )

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
                              .Width(120.0),
                           TextBlock().Text("分間").VerticalAlignmentCenter()
                        )
                        .HorizontalAlignmentRight(),
                     ValidationErrorsText.create
                        { Errors =
                           ctx.FormCtx.Errors |> R3.map AppConfigErrors.chooseStandardWorkTime
                          FontSize = None }
                  )

            SettingsExpanderItem(
               Content = "標準勤務時間",
               Description = "1日の標準的な勤務時間を分単位で設定します。",
               Footer = footer
            )
         )
         |> ignore

         expander)
