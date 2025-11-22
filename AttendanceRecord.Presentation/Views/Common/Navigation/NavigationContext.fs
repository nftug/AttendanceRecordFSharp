namespace AttendanceRecord.Presentation.Views.Common.Navigation

open System
open System.Threading
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils

type Route =
    { Path: string
      ViewFn: unit -> Avalonia.Controls.Control }

type NavigationContext =
    { CurrentRoute: Observable<Route>
      RegisterGuard: (CancellationToken -> Tasks.Task<bool>) -> IDisposable
      NavigateTo: string -> Tasks.Task<bool> }

module NavigationContext =
    let create
        (routes: Route list)
        (initialPath: string)
        (disposables: CompositeDisposable)
        : NavigationContext =
        let currentPath = R3.property initialPath |> R3.disposeWith disposables

        let currentRoute =
            currentPath
            |> R3.map (fun key ->
                routes
                |> List.tryFind (fun r -> r.Path = key)
                |> Option.defaultValue
                    { Path = ""
                      ViewFn = fun () -> Avalonia.Controls.Panel() })

        let guards = ResizeArray<CancellationToken -> Tasks.Task<bool>>()

        let registerGuard (guard: CancellationToken -> Tasks.Task<bool>) : IDisposable =
            guards.Add guard
            Disposable.Create(fun () -> guards.Remove guard |> ignore)

        let navigateTo (targetKey: string) : Tasks.Task<bool> =
            invokeTask disposables (fun ct ->
                task {
                    if currentPath.CurrentValue = targetKey then
                        return true
                    else
                        let mutable canProceed = true

                        for guard in guards do
                            if canProceed then
                                let! canNavigate = guard ct

                                if not canNavigate then
                                    canProceed <- false

                        if canProceed then
                            currentPath.Value <- targetKey

                        return canProceed
                })

        { CurrentRoute = currentRoute
          RegisterGuard = registerGuard
          NavigateTo = navigateTo }
