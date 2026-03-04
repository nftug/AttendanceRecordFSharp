namespace AttendanceRecord.Presentation

open System
open System.Reflection
open Avalonia.Controls
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Application
open AttendanceRecord.Shared

[<AutoOpen>]
module private MainWindowHelpers =
   let setWindowIcon (window: Window) =
      let assemblyName = Assembly.GetExecutingAssembly().GetName().Name

      use iconStream =
         NXUI.AssetLoader.Open(new Uri $"avares://{assemblyName}/Assets/app_icon.png")

      window.Icon <- new WindowIcon(new Avalonia.Media.Imaging.Bitmap(iconStream))

   let initialize (services: ServiceContainer) (window: Window) : Window =
      window.Loaded.Add(fun _ ->
         setWindowIcon window

         if not (services.SingleInstanceGuard.TryAcquireLock()) then
            task {
               match! services.NamedPipe.SendMessage NamedPipeMessage.showMainWindow with
               | Ok() -> ()
               | Error err -> eprintfn $"Failed to send message to existing instance: {err}"

               getApplicationLifetime().Shutdown()
            }
            |> ignore
         else
            services.NamedPipe.Receiver
            |> R3.subscribe (fun msg ->
               match msg.Content with
               | NamedPipeMessage.showMainWindow ->
                  window.Show()
                  window.Activate()
               | _ -> ())
            |> ignore)

      window

module MainWindow =
   open FluentAvalonia.UI.Windowing
   open NXUI.Extensions
   open AttendanceRecord.Presentation

   let create (services: ServiceContainer) : Window =
      AppWindow()
         .Title(getApplicationTitle ())
         .Width(1200)
         .Height(820)
         .WindowStartupLocationCenterScreen()
         .Content(MainView.create services)
      |> initialize services
