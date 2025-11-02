namespace AttendanceRecord.Presentation

open Avalonia
open Avalonia.Themes.Fluent
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI.Hosts

type MainWindow(services: ServiceContainer) =
    inherit HostWindow()

    do
        base.Title <- "Attendance Record"
        base.Width <- 1200.0
        base.Height <- 820.0
        base.WindowStartupLocation <- Controls.WindowStartupLocation.CenterScreen
        base.Content <- MainView.view services

type App() =
    inherit Application()

    override this.Initialize() = this.Styles.Add(FluentTheme())

    override this.OnFrameworkInitializationCompleted() =
        let services = ServiceContainer.create ()

        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- MainWindow services
        | _ -> ()

        base.OnFrameworkInitializationCompleted()

module Program =
    [<EntryPoint>]
    let main argv =
        AppBuilder.Configure<App>().UsePlatformDetect().StartWithClassicDesktopLifetime argv
