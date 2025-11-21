namespace AttendanceRecord.Presentation

open Avalonia
open Avalonia.Controls
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.Application

[<AutoOpen>]
module private MainWindowLogic =
    let initialize (singleInstanceGuard: SingleInstanceGuard) (window: Window) : Window =
        getApplicationLifetime()
            .ShutdownRequested.Add(fun _ -> singleInstanceGuard.ReleaseLock())

        window.Closing.AddHandler(fun sender e ->
            e.Cancel <- true
            sender :?> Window |> _.Hide())

        window.Loaded.Add(fun _ ->
            if not (singleInstanceGuard.TryAcquireLock()) then
                task {
                    let! _ =
                        MessageBox.show
                            { Title = "多重起動の防止"
                              Message = "このアプリケーションは既に起動しています。"
                              OkContent = Some "終了"
                              CancelContent = None
                              Buttons = MessageBoxButtons.Ok }
                            None

                    getApplicationLifetime().Shutdown()
                }
                |> ignore
            else
                let trayIcons = new TrayIcons()
                trayIcons.Add(AppTrayIcon.create window)
                TrayIcon.SetIcons(Application.Current, trayIcons))

        window

module MainWindow =
    open NXUI.Extensions
    open type NXUI.Builders
    open DialogHostAvalonia
    open Material.Icons.Avalonia
    open AttendanceRecord.Presentation

    let create (services: ServiceContainer) : Window =
        Window()
            .Title("Attendance Record")
            .Width(1200)
            .Height(820)
            .WindowStartupLocationCenterScreen()
            .Styles(AppStyles(), DialogHostStyles(), MaterialIconStyles null)
            .Content(MainView.create services)
        |> initialize services.SingleInstanceGuard
