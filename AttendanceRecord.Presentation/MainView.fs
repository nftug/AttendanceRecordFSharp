namespace AttendanceRecord.Presentation.Components

open Avalonia.FuncUI
open AttendanceRecord.Presentation.Compositions
open AttendanceRecord.Presentation.Features.HomePage.Components

module MainView =

    let view (services: ServiceContainer) : Component =
        Component(fun _ ->
            let toggleWork = services.ToggleWorkUseCase.Handle
            let toggleRest = services.ToggleRestUseCase.Handle

            HomePageView.view
                { StatusObservable = services.CurrentStatusStore.CurrentStatus
                  OnToggleWork = toggleWork
                  OnToggleRest = toggleRest })
