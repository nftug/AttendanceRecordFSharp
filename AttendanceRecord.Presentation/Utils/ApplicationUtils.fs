namespace AttendanceRecord.Presentation.Utils

open Avalonia
open Avalonia.Controls
open Avalonia.VisualTree
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Threading
open System
open System.Threading
open System.Threading.Tasks
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

    let getPlatformColors () : Platform.PlatformColorValues =
        Application.Current.PlatformSettings.GetColorValues()

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

    let withLifecycle<'t when 't :> Control>
        (create: CompositeDisposable -> Control -> 't)
        : Control =
        let mutable disposables: CompositeDisposable option = None
        let container = ContentControl()

        container.AttachedToVisualTree.Add(fun _ ->
            let d = new CompositeDisposable()
            disposables <- Some d
            let control = create d container
            container.Content <- control)

        container.DetachedFromVisualTree.Add(fun _ ->
            match disposables with
            | Some d ->
                d.Dispose()
                disposables <- None
            | None -> ())

        container

    let toChildren (items: #Control seq) : Control[] =
        items |> Seq.toArray |> Array.map (fun c -> c :> Control)

    let toView
        (render: CompositeDisposable -> Control -> 'a -> Control)
        (source: Observable<'a>)
        : Control =
        let container = ContentControl()
        let disposables = new CompositeDisposable()

        source
        |> R3.subscribe (fun v ->
            let child = render disposables container v
            container.Content <- child)
        |> disposables.Add

        container.DetachedFromVisualTree.Add(fun _ -> disposables.Dispose())

        container

    let toListView
        (itemTemplate: CompositeDisposable -> Control -> 'a -> Control)
        (source: Observable<'a list>)
        : Control =
        source
        |> toView (fun d s v ->
            let stackPanel = StackPanel()

            stackPanel.Children.AddRange(v |> List.map (itemTemplate d s) |> toChildren)

            stackPanel)

    let toOptView
        (render: CompositeDisposable -> Control -> 'a -> Control)
        (source: Observable<'a option>)
        : Control =
        source
        |> toView (fun d s ->
            function
            | Some v -> render d s v
            | None -> Panel())

    let toOptListView
        (itemTemplate: CompositeDisposable -> Control -> 'a -> Control)
        (source: Observable<'a option list>)
        : Control =
        source
        |> toView (fun d s vList ->
            let stackPanel = StackPanel()

            stackPanel.Children.AddRange(
                vList |> List.choose id |> List.map (itemTemplate d s) |> toChildren
            )

            stackPanel)
