open Avalonia
open NXUI.Extensions
open AttendanceRecord.Presentation

[<EntryPoint>]
let main argv =
    AppBuilder
        .Configure<Application>()
        .UsePlatformDetect()
        .UseFluentTheme()
        .UseR3()
        .WithApplicationName("Attendance Record")
        .StartWithClassicDesktopLifetime(MainWindow.create, argv)
