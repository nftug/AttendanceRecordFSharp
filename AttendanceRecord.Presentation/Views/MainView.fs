namespace AttendanceRecord.Presentation.Views

open type NXUI.Builders
open NXUI.Extensions
open Material.Icons
open DialogHostAvalonia

open AttendanceRecord.Shared
open AttendanceRecord.Presentation
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.HomePage
open AttendanceRecord.Presentation.Views.AboutPage
open AttendanceRecord.Presentation.Views.SettingsPage
open AttendanceRecord.Presentation.Views.Common

module MainView =
    type private PageKeys =
        | Home
        | Settings
        | About

    let view (services: ServiceContainer) : Avalonia.Controls.Control =
        withReactive (fun disposables _ ->
            let page = R3.property Home |> R3.disposeWith disposables

            let homePageContent =
                let statusObservable = services.CurrentStatusStore.CurrentStatus
                let toggleWork = services.ToggleWorkUseCase.Handle
                let toggleRest = services.ToggleRestUseCase.Handle

                HomePageView.view
                    { Status = statusObservable
                      OnToggleWork = toggleWork
                      OnToggleRest = toggleRest }

            let navigationView =
                NavigationView.create
                    { InitialPageKey = page.CurrentValue
                      OnPageSelected = (fun key -> page.Value <- key) |> Some
                      Pages =
                        Map
                            [ Home,
                              { Icon = MaterialIconKind.Home
                                Title = "ホーム"
                                View = homePageContent }
                              Settings,
                              { Icon = MaterialIconKind.Settings
                                Title = "設定"
                                View = SettingsPageView.view () }
                              About,
                              { Icon = MaterialIconKind.Info
                                Title = "このアプリについて"
                                View = AboutPageView.view () } ] }

            Grid()
                .Children(navigationView, WindowNotificationManager().PositionBottomCenter().MaxItems(1), DialogHost()))
