namespace AttendanceRecord.Presentation.Views.Common.Navigation

open System
open System.Threading
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils

type NavigationContext =
    { CurrentPageKey: ReadOnlyReactiveProperty<string>
      RegisterGuard: (CancellationToken -> Tasks.Task<bool>) -> IDisposable
      NavigateTo: string -> Tasks.Task<bool> }

[<AutoOpen>]
module NavigationContext =
    let createNavigationContext
        (initialPageKey: string)
        (disposables: CompositeDisposable)
        : NavigationContext =
        let currentPageKey = R3.property initialPageKey |> R3.disposeWith disposables

        let guards = ResizeArray<CancellationToken -> Tasks.Task<bool>>()

        let registerGuard (guard: CancellationToken -> Tasks.Task<bool>) : IDisposable =
            guards.Add guard
            Disposable.Create(fun () -> guards.Remove guard |> ignore)

        let navigateTo (targetKey: string) : Tasks.Task<bool> =
            invokeTask disposables (fun ct ->
                task {
                    if currentPageKey.CurrentValue = targetKey then
                        return true
                    else
                        let mutable canProceed = true

                        for guard in guards do
                            if canProceed then
                                let! canNavigate = guard ct

                                if not canNavigate then
                                    canProceed <- false

                        if canProceed then
                            currentPageKey.Value <- targetKey

                        return canProceed
                })

        { CurrentPageKey = currentPageKey
          RegisterGuard = registerGuard
          NavigateTo = navigateTo }
