open Avalonia
open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views
open AttendanceRecord.Presentation
open DialogHostAvalonia
open Material.Icons.Avalonia

let Build () =
    let services = ServiceContainer.create ()

    Window()
        .Title("Attendance Record")
        .Width(1200)
        .Height(820)
        .WindowStartupLocationCenterScreen()
        .Styles(AppStyles(), DialogHostStyles(), MaterialIconStyles null)
        .AddTrayIcon(services.SingleInstanceGuard)
        .Content(MainView.view services)

[<EntryPoint>]
let Main argv =
    AppBuilder
        .Configure<Application>()
        .UsePlatformDetect()
        .UseFluentTheme()
        .UseR3()
        .WithApplicationName("Attendance Record")
        .StartWithClassicDesktopLifetime(Build, argv)
    |> ignore

    0
