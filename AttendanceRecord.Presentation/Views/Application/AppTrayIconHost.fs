namespace AttendanceRecord.Presentation.Views.Application

open Avalonia
open Avalonia.Controls
open Avalonia.Media.Imaging
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common.Context

module AppTrayIconHost =
    let create () =
        withLifecycle (fun _ self ->
            let appCtx, _ = Context.require<ApplicationContext> self
            let window = appCtx.MainWindow

            let showAndActivateWindow () =
                window.Show()
                window.WindowState <- WindowState.Normal
                window.Activate()

            let nativeMenu = NativeMenu()

            let showMenuItem = new NativeMenuItem "表示"
            showMenuItem.Click.Add(fun _ -> showAndActivateWindow ())
            nativeMenu.Items.Add showMenuItem

            let exitMenuItem = new NativeMenuItem "終了"
            exitMenuItem.Click.Add(fun _ -> appCtx.ShutdownWithGuard())
            nativeMenu.Items.Add exitMenuItem

            use iconStream = EmbeddedResourceProvider.getFileStream "Assets/tray_icon.ico"

            let trayIcon =
                new TrayIcon(
                    ToolTipText = "AttendanceRecord",
                    Icon = WindowIcon(new Bitmap(iconStream)),
                    Menu = nativeMenu
                )

            trayIcon.Clicked.Add(fun _ -> showAndActivateWindow ())

            let trayIcons = new TrayIcons()
            trayIcons.Add trayIcon
            TrayIcon.SetIcons(Application.Current, trayIcons)

            Control())
