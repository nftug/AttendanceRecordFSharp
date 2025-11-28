namespace AttendanceRecord.Presentation.Views.Common.Context

open System
open System.Threading
open R3
open AttendanceRecord.Presentation.Utils
open Avalonia.Controls

type ApplicationContextProps = { HideOnClose: bool }

type ApplicationContext =
    { MainWindow: Window
      RegisterOnClosingGuard: (unit -> Tasks.Task<bool>) -> IDisposable }

module ApplicationContext =
    let create
        (props: ApplicationContextProps)
        (disposables: CompositeDisposable)
        : ApplicationContext =
        let mutable isShuttingDown = false
        let applicationLifetime = getApplicationLifetime ()
        let mainWindow = getMainWindow ()

        let closingGuards = ResizeArray<unit -> Tasks.Task<bool>>()

        let registerOnClosingGuard (guard: unit -> Tasks.Task<bool>) : IDisposable =
            closingGuards.Add guard
            Disposable.Create(fun () -> closingGuards.Remove guard |> ignore)

        applicationLifetime.ShutdownRequested.Add(fun _ -> isShuttingDown <- true)

        mainWindow.Closing.AddHandler(fun _ e ->
            if not isShuttingDown then
                e.Cancel <- true

                invokeTask disposables (fun ct ->
                    task {
                        let mutable canProceed = true

                        for guard in closingGuards do
                            if canProceed then
                                let! canClose = guard ()

                                if not canClose then
                                    canProceed <- false

                        if canProceed then
                            if props.HideOnClose then
                                mainWindow.Hide()
                            else
                                mainWindow.Close()

                    })
                |> ignore)

        { MainWindow = mainWindow
          RegisterOnClosingGuard = registerOnClosingGuard }
