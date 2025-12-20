open System
open Avalonia
open NXUI.Extensions
open AttendanceRecord.Presentation
open FluentAvalonia.Styling
open AttendanceRecord.Presentation.Utils

[<EntryPoint>]
[<STAThread>]
let main argv =
   let buildWindow () =
      let services = ServiceContainer.create ()
      MainWindow.create services

   AppBuilder
      .Configure<Application>()
      .UsePlatformDetect()
      .UseR3()
      .WithApplicationName(getApplicationTitle ())
      .AfterSetup(fun builder ->
         builder.Instance.Styles.Add(FluentAvaloniaTheme(PreferUserAccentColor = true)))
      .With(MacOSPlatformOptions(ShowInDock = false))
      .StartWithClassicDesktopLifetime(buildWindow, argv)
