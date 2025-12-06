namespace AttendanceRecord.Presentation.Views.Common.Context

open System
open System.Threading
open R3
open AttendanceRecord.Presentation.Utils
open Avalonia.Controls

type ApplicationContextProps = { HideOnClose: bool }

type ApplicationContext =
   { MainWindow: Window
     RegisterOnClosingGuard: (unit -> Tasks.Task<bool>) -> IDisposable
     ShutdownWithGuard: unit -> unit }

module ApplicationContext =
   let create (props: ApplicationContextProps) (_: CompositeDisposable) : ApplicationContext =

      let applicationLifetime = getApplicationLifetime ()
      let mainWindow = getMainWindow ()

      let mutable isShuttingDown = false

      let closingGuards = ResizeArray<unit -> Tasks.Task<bool>>()

      let registerOnClosingGuard (guard: unit -> Tasks.Task<bool>) : IDisposable =
         closingGuards.Add guard
         Disposable.Create(fun () -> closingGuards.Remove guard |> ignore)

      applicationLifetime.ShutdownRequested.Add(fun _ -> isShuttingDown <- true)

      let processClosingGuard () =
         task {
            let mutable canProceed = true

            for guard in closingGuards do
               if canProceed then
                  let! canClose = guard ()

                  if not canClose then
                     canProceed <- false

            return canProceed
         }

      mainWindow.Closing.AddHandler(fun _ e ->
         if not isShuttingDown then
            e.Cancel <- true

            task {
               let! canProceed = processClosingGuard ()

               if canProceed then
                  if props.HideOnClose then
                     mainWindow.Hide()
                  else
                     mainWindow.Close()
            }
            |> ignore)

      let shutdownWithGuard () =
         task {
            let! canProceed = processClosingGuard ()

            if canProceed then
               applicationLifetime.Shutdown()
         }
         |> ignore

      { MainWindow = mainWindow
        RegisterOnClosingGuard = registerOnClosingGuard
        ShutdownWithGuard = shutdownWithGuard }
