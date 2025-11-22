namespace AttendanceRecord.Presentation.Views.Application

open type NXUI.Builders
open NXUI.Extensions
open Material.Icons
open DialogHostAvalonia

open AttendanceRecord.Presentation
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.Common.Navigation
open AttendanceRecord.Presentation.Views.HomePage
open AttendanceRecord.Presentation.Views.HistoryPage
open AttendanceRecord.Presentation.Views.AboutPage
open AttendanceRecord.Presentation.Views.SettingsPage
open AttendanceRecord.Presentation.Utils

module private NavigationContext =
    let create (services: ServiceContainer) (content: Avalonia.Controls.Control) =
        withLifecycle (fun disposables _ ->
            let routes =
                [ { Path = "/"
                    ViewFn =
                      fun () ->
                          HomePageView.create
                              { Status = services.CurrentStatusStore.CurrentStatus
                                ToggleWork = services.ToggleWorkUseCase
                                ToggleRest = services.ToggleRestUseCase } }
                  { Path = "/history"
                    ViewFn =
                      fun () ->
                          HistoryPageView.create
                              { GetMonthlyWorkRecords = services.GetMonthlyWorkRecordsUseCase
                                GetWorkRecordDetails = services.GetWorkRecordDetailsUseCase
                                SaveWorkRecord = services.SaveWorkRecordUseCase
                                DeleteWorkRecord = services.DeleteWorkRecordUseCase } }
                  { Path = "/settings"
                    ViewFn =
                      fun () ->
                          SettingsPageView.create
                              { AppConfig = services.AppConfig
                                SaveAppConfig = services.SaveAppConfigUseCase } }
                  { Path = "/about"
                    ViewFn = fun () -> AboutPageView.create () } ]

            NavigationContext.create routes "/" disposables |> Context.provide content)

module MainView =
    let create (services: ServiceContainer) : Avalonia.Controls.Control =
        withLifecycle (fun disposables _ ->
            // Start alarm host
            AlarmHost.start services.AlarmService disposables

            let themeContext = ThemeContext.create services.AppConfig disposables

            let menuItems =
                [ { Path = "/"
                    Icon = MaterialIconKind.Home
                    Title = "ホーム" }
                  { Path = "/history"
                    Icon = MaterialIconKind.History
                    Title = "履歴" }
                  { Path = "/settings"
                    Icon = MaterialIconKind.Settings
                    Title = "設定" }
                  { Path = "/about"
                    Icon = MaterialIconKind.Info
                    Title = "このアプリについて" } ]

            Grid()
                .Children(
                    NavigationView.create menuItems,
                    WindowNotificationManager().PositionBottomCenter().MaxItems(1),
                    DialogHost()
                )
            |> NavigationContext.create services
            |> fun content -> Context.provide content themeContext)
