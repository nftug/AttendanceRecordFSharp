namespace AttendanceRecord.Presentation.Utils

open Avalonia
open Avalonia.Controls
open Avalonia.VisualTree
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Threading
open System
open System.Threading
open System.Threading.Tasks
open System.Reflection
open R3
open AttendanceRecord.Shared

[<AutoOpen>]
module ApplicationUtils =
   let getApplicationLifetime () : IClassicDesktopStyleApplicationLifetime =
      Application.Current.ApplicationLifetime
      |> function
         | :? IClassicDesktopStyleApplicationLifetime as lifetime -> lifetime
         | _ -> failwith "Application lifetime not found."

   let getMainWindow () : Window =
      getApplicationLifetime ()
      |> fun lifetime ->
         match lifetime.MainWindow |> Option.ofObj with
         | Some window -> window
         | _ -> failwith "Main window not found."

   let getControlFromMainWindow<'t when 't :> Control> () : 't =
      getMainWindow ()
      |> fun window ->
         match window.GetVisualDescendants() |> Seq.tryFind (fun c -> c :? 't) with
         | Some control -> control :?> 't
         | None -> failwith $"Control of type {typeof<'t>.FullName} not found."

   let getDynamicBrushResource (resourceKey: string) : Observable<Media.IBrush> =
      R3.fromEventHandlerUnit Application.Current.ActualThemeVariantChanged
      |> R3.prepend ()
      |> R3.map (fun () ->
         let variant = Application.Current.ActualThemeVariant

         let resource =
            Application.Current.Styles.TryGetResource(resourceKey, variant) |> snd

         match resource with
         | :? Media.IBrush as brush -> brush
         | _ -> null)

   let getApplicationTitle () : string =
      Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title

   let nextTick (work: unit -> unit) : unit =
      Dispatcher.UIThread.Post(work, DispatcherPriority.Loaded)

   let asBinding<'T> (source: Observable<'T>) : Data.IBinding =
      source.AsSystemObservable().ToBinding()

   let invokeTask
      (disposables: CompositeDisposable)
      (work: CancellationToken -> Task<'a>)
      : Task<'a> =
      task {
         let cts = new CancellationTokenSource()
         disposables.Add(Disposable.Create(fun () -> cts.Cancel()))

         try
            return! work cts.Token
         with
         | :? OperationCanceledException -> return Unchecked.defaultof<'a>
         | :? ObjectDisposedException -> return Unchecked.defaultof<'a>
      }

   let withLifecycle (create: CompositeDisposable -> ContentControl -> #Control) : ContentControl =
      let mutable disposables: CompositeDisposable option = None
      let container = ContentControl()

      container.AttachedToVisualTree.Add(fun _ ->
         let d = new CompositeDisposable()
         disposables <- Some d
         container.Content <- create d container)

      container.DetachedFromVisualTree.Add(fun _ ->
         disposables
         |> Option.iter (fun d ->
            d.Dispose()
            disposables <- None))

      container

   let toChildren (items: #Control seq) : Control[] =
      items |> Seq.toArray |> Array.map (fun c -> c :> Control)

   let toView
      (render: CompositeDisposable -> ContentControl -> 'a -> #Control)
      (source: Observable<'a>)
      : ContentControl =
      let container = ContentControl()
      let disposables = new CompositeDisposable()

      source
      |> R3.subscribe (fun v ->
         let child = render disposables container v
         container.Content <- child)
      |> disposables.Add

      container.DetachedFromVisualTree.Add(fun _ -> disposables.Dispose())

      container
