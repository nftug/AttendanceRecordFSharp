namespace AttendanceRecord.Presentation.Layout.Components

open Avalonia.FuncUI
open Material.Icons
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
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
                TextBlock.create
                    [ TextBlock.text "Settings Page"
                      TextBlock.fontSize 24.0
                      TextBlock.horizontalAlignment HorizontalAlignment.Center
                      TextBlock.verticalAlignment VerticalAlignment.Center ]

            let aboutPageContent =
                TextBlock.create
                    [ TextBlock.text "About Page"
                      TextBlock.fontSize 24.0
                      TextBlock.horizontalAlignment HorizontalAlignment.Center
                      TextBlock.verticalAlignment VerticalAlignment.Center ]

            NavigationView.view
                { PageKey = page
                  PageItems =
                    Map
                        [ HomePage,
                          { Icon = MaterialIconKind.HomeOutline
                            Title = "Home"
                            Content = homePageContent }
                          SettingsPage,
                          { Icon = MaterialIconKind.CogOutline
                            Title = "Settings"
                            Content = settingPageContent }
                          AboutPage,
                          { Icon = MaterialIconKind.InformationOutline
                            Title = "About"
                            Content = aboutPageContent } ] })
