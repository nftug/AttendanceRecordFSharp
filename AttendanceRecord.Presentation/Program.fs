namespace AttendanceRecord.Presentation

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Controls.Primitives
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Styling
open Avalonia.Controls
open Avalonia.Layout
open Material.Icons.Avalonia
open AttendanceRecord.Presentation.Layout.Components

type MainWindow(services: ServiceContainer) =
    inherit HostWindow()

    do
        base.Title <- "Attendance Record"
        base.Width <- 1200.0
        base.Height <- 820.0
        base.WindowStartupLocation <- WindowStartupLocation.CenterScreen
        base.Content <- MainView.view services

type App() =
    inherit Application()

    let getMyStyles () =
        Styles()
        |> fun s ->
            let buttonStyle = Style(fun x -> x.OfType<Button>())
            buttonStyle.Add(Setter(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center))
            buttonStyle.Add(Setter(Button.VerticalContentAlignmentProperty, VerticalAlignment.Center))
            s.Add buttonStyle

            let toggleButtonStyle = Style(fun x -> x.OfType<ToggleButton>())
            toggleButtonStyle.Add(Setter(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center))
            toggleButtonStyle.Add(Setter(Button.VerticalContentAlignmentProperty, VerticalAlignment.Center))
            s.Add toggleButtonStyle

            s

    override this.Initialize() =
        this.Styles.Add(FluentTheme())
        this.Styles.Add(MaterialIconStyles null)
        this.Styles.Add(getMyStyles ())

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
