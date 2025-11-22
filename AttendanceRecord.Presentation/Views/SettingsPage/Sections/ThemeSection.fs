namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open R3
open Avalonia.Media
open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared

module ThemeSection =
    let create () =
        withLifecycle (fun disposables self ->
            let ctx = SettingsPageContextProvider.require self |> fst
            let themeCtx = Context.require<ThemeContext> self |> fst

            let createRadioButton (text: string) (theme: ThemeMode) =
                RadioButton()
                    .Content(text)
                    .GroupName("ThemeGroup")
                    .IsChecked(ctx.Form |> R3.map (fun f -> f.ThemeMode = theme) |> asBinding)
                    .Margin(0.0, 0.0, 20.0, 0.0)
                    .OnIsCheckedChangedHandler(fun ctl _ ->
                        if ctl.IsChecked.HasValue && ctl.IsChecked.Value then
                            ctx.Form.Value <-
                                { ctx.Form.Value with
                                    ThemeMode = theme }

                            themeCtx.ThemeMode.Value <- theme)

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(20.0)
                .Child(
                    StackPanel()
                        .Spacing(15.0)
                        .Children(
                            TextBlock().Text("テーマ設定").FontSize(18.0).FontWeightBold(),
                            StackPanel()
                                .OrientationHorizontal()
                                .Spacing(10.0)
                                .Children(
                                    createRadioButton "システム" SystemTheme,
                                    createRadioButton "ライト" LightTheme,
                                    createRadioButton "ダーク" DarkTheme
                                )
                        )
                ))
