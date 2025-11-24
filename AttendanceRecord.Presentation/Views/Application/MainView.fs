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
    let provide (services: ServiceContainer) (content: Avalonia.Controls.Control) =
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
                            DeleteWorkRecord = services.DeleteWorkRecordUseCase
                            CurrentStatusStore = services.CurrentStatusStore } }
              { Path = "/settings"
                ViewFn =
                  fun () ->
                      SettingsPageView.create
                          { AppConfig = services.AppConfig
                            SaveAppConfig = services.SaveAppConfigUseCase } }
              { Path = "/about"
                ViewFn = fun () -> AboutPageView.create () } ]

        Context.provideWithBuilder (NavigationContext.create routes "/") content

module MainView =
    let create (services: ServiceContainer) =
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

        Panel()
            .Children(
                NavigationView.create menuItems,
                AlarmHost.create services.AlarmService,
                WindowNotificationManager().PositionBottomCenter().MaxItems(1),
                DialogHost()
            )
        |> NavigationContext.provide services
        |> Context.provideWithBuilder (ThemeContext.create services.AppConfig)
