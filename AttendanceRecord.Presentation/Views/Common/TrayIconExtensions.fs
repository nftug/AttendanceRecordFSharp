namespace AttendanceRecord.Presentation.Views.Common

open Avalonia
open Avalonia.Controls
open Avalonia.Media.Imaging
open System
open System.Runtime.CompilerServices
open AttendanceRecord.Presentation.Utils

open System.ComponentModel

module private TrayIcon =
    let createTrayIcon () : TrayIcon =
        let showAndActivateWindow () =
            let window = getMainWindow ()
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
        let iconUri = Uri "avares://AttendanceRecord/Assets/tray_icon.ico"
        trayIcon.Icon <- new WindowIcon(new Bitmap(NXUI.AssetLoader.Open iconUri))
        trayIcon.Menu <- nativeMenu
        trayIcon

[<Extension>]
type TrayIconExtensions =
    [<Extension>]
    static member AddTrayIcon(window: Window) : Window =
        let trayIcons = new TrayIcons()
        trayIcons.Add(TrayIcon.createTrayIcon ())

        TrayIcon.SetIcons(Application.Current, trayIcons)

        let handleClosingRequested (sender: obj) (e: CancelEventArgs) =
            e.Cancel <- true
            sender :?> Window |> _.Hide()

        getApplicationLifetime().ShutdownRequested.AddHandler handleClosingRequested
        window.Closing.AddHandler handleClosingRequested

        window
