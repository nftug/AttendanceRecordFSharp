namespace AttendanceRecord.Presentation.Layout.Components

open Avalonia.FuncUI
open Material.Icons
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Controls.Notifications
open AttendanceRecord.Presentation.Features.HomePage.Components
open AttendanceRecord.Presentation

module MainView =
    type private PageKeys =
        | HomePage
        | SettingsPage
        | AboutPage

    let view (services: ServiceContainer) : Component =
        Component(fun ctx ->
            let page = ctx.useState HomePage

            let homePageContent =
                let statusObservable = services.CurrentStatusStore.CurrentStatus
                let toggleWork = services.ToggleWorkUseCase.Handle
                let toggleRest = services.ToggleRestUseCase.Handle

                HomePageView.view
                    { StatusObservable = statusObservable
                      OnToggleWork = toggleWork
                      OnToggleRest = toggleRest }

            let settingPageContent =
                CjkTextBlock.create
                    [ TextBlock.text "設定"
                      TextBlock.fontSize 24.0
                      TextBlock.horizontalAlignment HorizontalAlignment.Center
                      TextBlock.verticalAlignment VerticalAlignment.Center ]

            let aboutPageContent =
                CjkTextBlock.create
                    [ TextBlock.text "このアプリについて"
                      TextBlock.fontSize 24.0
                      TextBlock.horizontalAlignment HorizontalAlignment.Center
                      TextBlock.verticalAlignment VerticalAlignment.Center ]

            let navigationView =
                NavigationView.view
                    { PageKey = page
                      PageItems =
                        Map
                            [ HomePage,
                              { Icon = MaterialIconKind.HomeOutline
                                Title = "ホーム"
                                Content = homePageContent }
                              SettingsPage,
                              { Icon = MaterialIconKind.CogOutline
                                Title = "設定"
                                Content = settingPageContent }
                              AboutPage,
                              { Icon = MaterialIconKind.InformationOutline
                                Title = "このアプリについて"
                                Content = aboutPageContent } ] }

            Grid.create
                [ Grid.children
                      [ navigationView
                        WindowNotificationManager.create
                            [ WindowNotificationManager.position NotificationPosition.BottomCenter
                              WindowNotificationManager.maxItems 1 ]
                        DialogHost.create [] ] ])
