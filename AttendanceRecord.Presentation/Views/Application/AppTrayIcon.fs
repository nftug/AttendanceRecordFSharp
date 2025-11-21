namespace AttendanceRecord.Presentation.Views.Application

open Avalonia.Controls
open Avalonia.Media.Imaging
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common

module AppTrayIcon =
    let create (window: Window) : TrayIcon =
        let showAndActivateWindow () =
            window.Show()
            window.WindowState <- WindowState.Normal
            window.Activate()

        let confirmAndExitApp () =
            task {
                showAndActivateWindow ()

                let! result =
                    MessageBox.show
                        { Title = "アプリケーションの終了確認"
                          Message = "本当にアプリケーションを終了しますか？"
                          OkContent = Some "終了"
                          CancelContent = None
                          Buttons = MessageBoxButtons.OkCancel }
                        None

                if result then
                    getApplicationLifetime().Shutdown()
            }
            |> ignore

        let nativeMenu = NativeMenu()

        let showMenuItem = new NativeMenuItem "表示"
        showMenuItem.Click.Add(fun _ -> showAndActivateWindow ())
        nativeMenu.Items.Add showMenuItem

        let exitMenuItem = new NativeMenuItem "終了"
        exitMenuItem.Click.Add(fun _ -> confirmAndExitApp ())
        nativeMenu.Items.Add exitMenuItem

        let trayIcon = new TrayIcon()
        trayIcon.ToolTipText <- "AttendanceRecord"

        use iconStream = EmbeddedResourceProvider.getFileStream "Assets/tray_icon.ico"
        trayIcon.Icon <- new WindowIcon(new Bitmap(iconStream))

        trayIcon.Menu <- nativeMenu
        trayIcon.Clicked.Add(fun _ -> showAndActivateWindow ())
        trayIcon
