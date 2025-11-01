namespace AttendanceRecord.Presentation

open Avalonia.FuncUI
open AttendanceRecord.Presentation.Features.HomePage.Components

module MainView =
    let view (services: ServiceContainer) : Component =
        Component(fun _ ->
            let statusObservable = services.CurrentStatusStore.CurrentStatus
            let toggleWork = services.ToggleWorkUseCase.Handle
            let toggleRest = services.ToggleRestUseCase.Handle

            HomePageView.view
                { StatusObservable = statusObservable
                  OnToggleWork = toggleWork
                  OnToggleRest = toggleRest })
