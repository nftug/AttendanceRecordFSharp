namespace AttendanceRecord.Presentation

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Material.Icons.Avalonia
open DialogHostAvalonia
open AttendanceRecord.Presentation.Layout.Components

type MainWindow(services: ServiceContainer) as this =
    inherit HostWindow()

    do
        this.Title <- "Attendance Record"
        this.Width <- 1200.0
        this.Height <- 820.0
        this.WindowStartupLocation <- WindowStartupLocation.CenterScreen
        this.Content <- MainView.view services

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add(FluentTheme())
        this.Styles.Add(MaterialIconStyles null)
        this.Styles.Add(DialogHostStyles())
        this.Styles.Add(AppStyles())

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
