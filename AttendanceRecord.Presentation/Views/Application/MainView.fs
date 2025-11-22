namespace AttendanceRecord.Presentation.Views.Application

open type NXUI.Builders
open NXUI.Extensions
open Material.Icons
open DialogHostAvalonia

open AttendanceRecord.Presentation
open AttendanceRecord.Presentation.Views.Common.Navigation
open AttendanceRecord.Presentation.Views.HomePage
open AttendanceRecord.Presentation.Views.HistoryPage
open AttendanceRecord.Presentation.Views.AboutPage
open AttendanceRecord.Presentation.Views.SettingsPage
open AttendanceRecord.Presentation.Utils

module MainView =
    let create (services: ServiceContainer) : Avalonia.Controls.Control =
        withLifecycle (fun disposables _ ->
            // Start alarm host
            AlarmHost.start services.AlarmService disposables

            Grid()
                .Children(
                    NavigationView.create
                        { InitialPageKey = "/"
                          OnPageSelected = None
                          Pages =
                            [ { Key = "/"
                                Icon = MaterialIconKind.Home
                                Title = "ホーム"
                                View =
                                  fun () ->
                                      HomePageView.create
                                          { Status = services.CurrentStatusStore.CurrentStatus
                                            ToggleWork = services.ToggleWorkUseCase
                                            ToggleRest = services.ToggleRestUseCase } }
                              { Key = "/history"
                                Icon = MaterialIconKind.History
                                Title = "履歴"
                                View =
                                  fun () ->
                                      HistoryPageView.create
                                          { GetMonthlyWorkRecords =
                                              services.GetMonthlyWorkRecordsUseCase
                                            GetWorkRecordDetails =
                                              services.GetWorkRecordDetailsUseCase
                                            SaveWorkRecord = services.SaveWorkRecordUseCase
                                            DeleteWorkRecord = services.DeleteWorkRecordUseCase } }
                              { Key = "/settings"
                                Icon = MaterialIconKind.Settings
                                Title = "設定"
                                View = fun () -> SettingsPageView.create () }
                              { Key = "/about"
                                Icon = MaterialIconKind.Info
                                Title = "このアプリについて"
                                View = fun () -> AboutPageView.create () } ] },
                    WindowNotificationManager().PositionBottomCenter().MaxItems(1),
                    DialogHost()
                ))
