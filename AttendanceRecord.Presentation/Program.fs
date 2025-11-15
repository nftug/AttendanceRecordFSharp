open Avalonia
open NXUI.Extensions
open AttendanceRecord.Presentation

[<EntryPoint>]
let main argv =
    let buildWindow () =
        let services = ServiceContainer.create ()
        MainWindow.create services

    AppBuilder
        .Configure<Application>()
        .UsePlatformDetect()
        .UseFluentTheme()
        .UseR3()
        .WithApplicationName("Attendance Record")
        .StartWithClassicDesktopLifetime(buildWindow, argv)
