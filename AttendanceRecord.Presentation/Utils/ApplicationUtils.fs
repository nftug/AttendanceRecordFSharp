namespace AttendanceRecord.Presentation.Utils

open Avalonia
open Avalonia.Controls
open Avalonia.VisualTree
open Avalonia.Controls.ApplicationLifetimes
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

    let asBinding<'T> (source: Observable<'T>) : Data.IBinding =
        source.AsSystemObservable().ToBinding()

    let invokeTask
        (disposables: CompositeDisposable)
        (work: CancellationToken -> Task<unit>)
        : Task<unit> =
        task {
            let cts = new CancellationTokenSource()
            disposables.Add(Disposable.Create(fun () -> cts.Cancel()))

            try
                do! work cts.Token
            with
            | :? OperationCanceledException -> ()
            | :? ObjectDisposedException -> ()
        }

    let withReactive<'t when 't :> Control>
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

    let toChildren (items: #Control list) : Control[] =
        items |> List.toArray |> Array.map (fun c -> c :> Control)

    let toView (render: 'a -> Control) (source: Observable<'a>) : Control =
        let container = ContentControl()
        let subscription = source |> R3.subscribe (fun v -> container.Content <- render v)

        container.DetachedFromVisualTree.Add(fun _ -> subscription.Dispose())

        container

    let toViewWithReactive
        (render: 'a -> CompositeDisposable -> Control -> Control)
        (source: Observable<'a>)
        : Control =
        withReactive (fun disposables self -> source |> toView (fun v -> render v disposables self))
