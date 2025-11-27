namespace AttendanceRecord.Presentation.Views.Application

open Avalonia.Controls
open Avalonia.Media.Imaging
open AttendanceRecord.Presentation.Utils

module AppTrayIcon =
    let create (window: Window) : TrayIcon =
        let showAndActivateWindow () =
            window.Show()
            window.WindowState <- WindowState.Normal
            window.Activate()

        let nativeMenu = NativeMenu()

        let showMenuItem = new NativeMenuItem "表示"
        showMenuItem.Click.Add(fun _ -> showAndActivateWindow ())
        nativeMenu.Items.Add showMenuItem

        let exitMenuItem = new NativeMenuItem "終了"
        exitMenuItem.Click.Add(fun _ -> getApplicationLifetime().Shutdown())
        nativeMenu.Items.Add exitMenuItem

        use iconStream = EmbeddedResourceProvider.getFileStream "Assets/tray_icon.ico"

        let trayIcon =
            new TrayIcon(
                ToolTipText = "AttendanceRecord",
                Icon = WindowIcon(new Bitmap(iconStream)),
                Menu = nativeMenu
            )

        trayIcon.Clicked.Add(fun _ -> showAndActivateWindow ())

        trayIcon
