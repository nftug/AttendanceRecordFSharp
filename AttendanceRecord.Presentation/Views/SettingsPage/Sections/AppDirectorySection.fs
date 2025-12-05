namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open System
open NXUI.Extensions
open type NXUI.Builders
open FluentAvalonia.UI.Controls
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.Common.Context
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared

module AppDirectorySection =
    let create () =
        withLifecycle (fun _ self ->
            let ctx = Context.require<SettingsPageContext> self |> fst
            let appCtx = Context.require<ApplicationContext> self |> fst

            let handleOpenAppDirectory () =
                let window = appCtx.MainWindow
                let directoryPath = ctx.AppDirectoryPath
                let uri = Uri(directoryPath, UriKind.Absolute)
                window.Launcher.LaunchUriAsync uri |> ignore

            let footer =
                Button()
                    .Content(
                        SymbolIconLabel.create
                            { Symbol = Symbol.Folder |> R3.ret
                              Label = "フォルダを開く" |> R3.ret
                              Spacing = Some 8.0 |> R3.ret }
                    )
                    .Width(150.0)
                    .OnClickHandler(fun _ _ -> handleOpenAppDirectory ())

            SettingsExpander(
                Header = "設定フォルダ",
                Description = "アプリケーションのデータが保存されるディレクトリを開きます。",
                IconSource = SymbolIconSource(Symbol = Symbol.Folder),
                Footer = footer
            ))
