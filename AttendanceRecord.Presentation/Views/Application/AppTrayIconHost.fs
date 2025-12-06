namespace AttendanceRecord.Presentation.Views.Application

open System
open System.Reflection
open Avalonia
open Avalonia.Platform
open Avalonia.Controls
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common.Context
open AttendanceRecord.Shared

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

         let trayIcon = new TrayIcon(ToolTipText = getApplicationTitle (), Menu = nativeMenu)
         trayIcon.Clicked.Add(fun _ -> showAndActivateWindow ())

         let trayIcons = new TrayIcons()
         trayIcons.Add trayIcon
         TrayIcon.SetIcons(Application.Current, trayIcons)

         window.Loaded.Add(fun _ ->
            // AssetLoader can only be used after Avalonia is initialized
            let assemblyName = Assembly.GetExecutingAssembly().GetName().Name

            R3.everyValueChanged Application.Current.PlatformSettings _.GetColorValues()
            |> R3.subscribe (fun colorValues ->
               let iconFileName =
                  match colorValues.ThemeVariant with
                  | PlatformThemeVariant.Dark -> "tray_icon_dark.png"
                  | _ -> "tray_icon_light.png"

               use iconStream =
                  NXUI.AssetLoader.Open(new Uri $"avares://{assemblyName}/Assets/{iconFileName}")

               trayIcon.Icon <- WindowIcon(new Media.Imaging.Bitmap(iconStream)))
            |> ignore)

         Control())
