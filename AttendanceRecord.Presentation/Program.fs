open Avalonia
open NXUI.Extensions
open AttendanceRecord.Presentation.Views

[<EntryPoint>]
let Main argv =
    AppBuilder
        .Configure<Application>()
        .UsePlatformDetect()
        .UseFluentTheme()
        .UseR3()
        .WithApplicationName("Attendance Record")
        .StartWithClassicDesktopLifetime(MainWindow.create, argv)
    |> ignore

    0
