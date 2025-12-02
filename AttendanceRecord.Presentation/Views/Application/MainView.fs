namespace AttendanceRecord.Presentation.Views.Application

open type NXUI.Builders
open NXUI.Extensions
open FluentAvalonia.UI.Controls

open AttendanceRecord.Presentation
open AttendanceRecord.Presentation.Views.Common.Context
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
                          { Status = services.WorkStatusStore.WorkStatus
                            AppConfig = services.AppConfig
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
                            WorkStatusStore = services.WorkStatusStore } }
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
        withLifecycle (fun disposables self ->
            let appCtx, _ = Context.require<ApplicationContext> self
            let navigationCtx, _ = Context.require<NavigationContext> self

            appCtx.RegisterOnClosingGuard(fun () -> navigationCtx.NavigateTo "/")
            |> disposables.Add

            Panel()
                .Children(
                    NavigationView.create
                        { MenuItems =
                            [ NavigationViewItem(
                                  Content = "ホーム",
                                  IconSource = SymbolIconSource(Symbol = Symbol.Home),
                                  Tag = "/"
                              )
                              NavigationViewItem(
                                  Content = "履歴",
                                  IconSource = SymbolIconSource(Symbol = Symbol.CalendarMonth),
                                  Tag = "/history"
                              ) ]
                          FooterMenuItems =
                            [ NavigationViewItem(
                                  Content = "設定",
                                  IconSource = SymbolIconSource(Symbol = Symbol.Settings),
                                  Tag = "/settings"
                              )
                              NavigationViewItem(
                                  Content = "このアプリについて",
                                  IconSource = SymbolIconSource(Symbol = Symbol.Help),
                                  Tag = "/about"
                              ) ] },
                    AppTrayIconHost.create (),
                    AlarmHost.create services.AlarmService,
                    WindowNotificationManager().PositionBottomCenter().MaxItems(1)
                ))
        |> NavigationContext.provide services
        |> Context.provideWithBuilder (ThemeContext.create services.AppConfig)
        |> Context.provideWithBuilder (ApplicationContext.create { HideOnClose = true })
