namespace AttendanceRecord.Presentation

open Avalonia
open Avalonia.Controls
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Application
open AttendanceRecord.Shared

[<AutoOpen>]
module private MainWindowHelpers =
    let initialize (services: ServiceContainer) (window: Window) : Window =
        getApplicationLifetime()
            .ShutdownRequested.Add(fun _ -> services.SingleInstanceGuard.ReleaseLock())

        window.Closing.AddHandler(fun sender e ->
            e.Cancel <- true
            sender :?> Window |> _.Hide())

        window.Loaded.Add(fun _ ->
            if not (services.SingleInstanceGuard.TryAcquireLock()) then
                task {
                    match! services.NamedPipe.SendMessage NamedPipeMessage.showMainWindow with
                    | Ok() -> ()
                    | Error err -> eprintfn $"Failed to send message to existing instance: {err}"

                    getApplicationLifetime().Shutdown()
                }
                |> ignore
            else
                let trayIcons = new TrayIcons()
                trayIcons.Add(AppTrayIcon.create window)
                TrayIcon.SetIcons(Application.Current, trayIcons)

                services.NamedPipe.Receiver
                |> R3.subscribe (fun msg ->
                    match msg.Content with
                    | NamedPipeMessage.showMainWindow ->
                        window.Show()
                        window.Activate()
                    | _ -> ())
                |> ignore)

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
        |> initialize services
