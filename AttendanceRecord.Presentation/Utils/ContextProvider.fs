namespace AttendanceRecord.Presentation.Utils

open System
open Avalonia.Controls
open Avalonia.VisualTree
open R3

type ContextProvider<'T when 'T: not struct>() =
    inherit ContentControl()

    let mutable value: 'T = Unchecked.defaultof<'T>
    let mutable disposables = new CompositeDisposable()
    let mutable builder: (CompositeDisposable -> 'T) option = None

    // === constructor helpers ===
    member this.Init(v: 'T, content: Control, ?name: string) =
        value <- v
        this.Content <- content
        this.Name <- defaultArg name this.Name
        this

    member this.Init(b: CompositeDisposable -> 'T, content: Control, ?name: string) =
        builder <- Some b
        this.Content <- content
        this.Name <- defaultArg name this.Name
        this

    // === lifecycle ===
    override _.OnAttachedToVisualTree e =
        base.OnAttachedToVisualTree e
        disposables <- new CompositeDisposable()

        match builder with
        | Some b -> value <- b disposables
        | None -> ()

    override _.OnDetachedFromVisualTree e =
        base.OnDetachedFromVisualTree e
        disposables.Dispose()
        disposables <- new CompositeDisposable()

        match box value with
        | :? IDisposable as d -> d.Dispose()
        | _ -> ()

    // === internal accessors ===
    member internal _.ValueInternal = value
    member internal _.DisposablesInternal = disposables

module Context =
    // === provide functions ===
    let provide<'T when 'T: not struct> (content: Control) (value: 'T) : ContextProvider<'T> =
        ContextProvider<'T>().Init(value, content)

    let provideWithBuilder<'T when 'T: not struct>
        (content: Control)
        (builder: CompositeDisposable -> 'T)
        : ContextProvider<'T> =
        ContextProvider<'T>().Init(builder, content)

    let provideWithName<'T when 'T: not struct>
        (content: Control)
        (name: string)
        (value: 'T)
        : ContextProvider<'T> =
        ContextProvider<'T>().Init(value, content, name)

    let provideWithBuilderAndName<'T when 'T: not struct>
        (content: Control)
        (name: string)
        (builder: CompositeDisposable -> 'T)
        : ContextProvider<'T> =
        ContextProvider<'T>().Init(builder, content, name)

    // === require / resolve functions ===
    let private findContextFromAncestors<'T when 'T: not struct>
        (control: Control)
        (name: string option)
        : ContextProvider<'T> option =
        control.GetVisualAncestors()
        |> Seq.tryPick (fun c ->
            match c with
            | :? ContextProvider<'T> as ctx -> Some ctx
            | _ -> None)
        |> Option.filter (fun ctx ->
            match name with
            | Some n -> ctx.Name = n
            | None -> true)

    let resolve<'T when 'T: not struct> (control: Control) : ('T * CompositeDisposable) option =
        match findContextFromAncestors control None with
        | None -> None
        | Some ctx -> Some(ctx.ValueInternal, ctx.DisposablesInternal)

    let require<'T when 'T: not struct> (control: Control) : ('T * CompositeDisposable) =
        match findContextFromAncestors control None with
        | None -> invalidOp $"Context<{typeof<'T>.Name}> not found in visual tree."
        | Some ctx -> ctx.ValueInternal, ctx.DisposablesInternal

    let resolveWithName<'T when 'T: not struct>
        (control: Control)
        (name: string)
        : ('T option * CompositeDisposable option) =
        match findContextFromAncestors<'T> control (Some name) with
        | None -> None, None
        | Some ctx -> Some ctx.ValueInternal, Some ctx.DisposablesInternal

    let requireWithName<'T when 'T: not struct>
        (control: Control)
        (name: string)
        : ('T * CompositeDisposable) =
        match findContextFromAncestors<'T> control (Some name) with
        | None ->
            invalidOp $"Context<{typeof<'T>.Name}> with name '{name}' not found in visual tree."
        | Some ctx -> ctx.ValueInternal, ctx.DisposablesInternal
