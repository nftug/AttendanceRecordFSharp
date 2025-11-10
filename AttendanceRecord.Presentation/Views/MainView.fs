namespace AttendanceRecord.Presentation.Views

open type NXUI.Builders
open NXUI.Extensions
open Material.Icons
open DialogHostAvalonia

open AttendanceRecord.Presentation
open AttendanceRecord.Presentation.Views.HomePage
open AttendanceRecord.Presentation.Views.AboutPage
open AttendanceRecord.Presentation.Views.SettingsPage
open AttendanceRecord.Presentation.Views.Common

module MainView =
    type private PageKeys =
        | Home
        | Settings
        | About

    let create (services: ServiceContainer) : Avalonia.Controls.Control =
        Grid()
            .Children(
                NavigationView.create
                    { InitialPageKey = Home
                      OnPageSelected = None
                      Pages =
                        Map
                            [ Home,
                              { Icon = MaterialIconKind.Home
                                Title = "ホーム"
                                View =
                                  HomePageView.create
                                      { Status = services.CurrentStatusStore.CurrentStatus
                                        OnToggleWork = services.ToggleWorkUseCase.Handle
                                        OnToggleRest = services.ToggleRestUseCase.Handle } }
                              Settings,
                              { Icon = MaterialIconKind.Settings
                                Title = "設定"
                                View = SettingsPageView.create () }
                              About,
                              { Icon = MaterialIconKind.Info
                                Title = "このアプリについて"
                                View = AboutPageView.create () } ] },
                WindowNotificationManager().PositionBottomCenter().MaxItems(1),
                DialogHost()
            )
