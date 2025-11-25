open Avalonia
open NXUI.Extensions
open AttendanceRecord.Presentation
open FluentAvalonia.Styling

[<EntryPoint>]
let main argv =
    let buildWindow () =
        let services = ServiceContainer.create ()
        MainWindow.create services

    AppBuilder
        .Configure<Application>()
        .UsePlatformDetect()
        .UseR3()
        .WithApplicationName("Attendance Record")
        .AfterSetup(fun builder ->
            builder.Instance.Styles.Add(
                FluentAvaloniaTheme(PreferSystemTheme = true, PreferUserAccentColor = true)
            ))
        .StartWithClassicDesktopLifetime(buildWindow, argv)
