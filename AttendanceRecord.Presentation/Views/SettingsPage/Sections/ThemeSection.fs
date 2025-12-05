namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open R3
open NXUI.Extensions
open FluentAvalonia.UI.Controls
open Avalonia.Controls
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common.Context
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Application.Dtos.Enums
open AttendanceRecord.Shared

module ThemeSection =
    let create () =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<SettingsPageContext> self
            let themeCtx = Context.require<ThemeContext> self |> fst

            self.DetachedFromVisualTree.Add(fun _ -> themeCtx.LoadFromConfig())

            let formThemeMode =
                ctx.FormCtx.Form |> R3.map _.ThemeMode |> R3.distinctUntilChanged

            formThemeMode
            |> R3.subscribe (fun themeMode -> themeCtx.ThemeMode.Value <- themeMode)
            |> disposables.Add

            let footer =
                formThemeMode
                |> toView (fun _ _ selectedTheme ->
                    ComboBox()
                        .ItemsSource(ThemeModeEnum.all)
                        .Width(170.0)
                        .SelectedItem(selectedTheme)
                        .ItemTemplateFunc(fun (item: ThemeModeEnum) ->
                            TextBlock().Text(ThemeModeEnum.toDisplayName item))
                        .OnSelectionChangedHandler(fun ctl _ ->
                            match ctl.SelectedItem with
                            | :? ThemeModeEnum as theme ->
                                ctx.FormCtx.Form.Value <-
                                    { ctx.FormCtx.Form.Value with
                                        ThemeMode = theme }
                            | _ -> ()))

            SettingsExpander(
                Header = "テーマ設定",
                Description = "アプリケーションのテーマに関する設定を行います。",
                IconSource = SymbolIconSource(Symbol = Symbol.DarkTheme),
                Footer = footer
            ))
