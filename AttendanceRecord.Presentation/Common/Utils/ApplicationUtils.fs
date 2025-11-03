namespace Avalonia

open Avalonia.VisualTree
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes

[<AutoOpen>]
module ApplicationUtils =
    let tryGetDesktopLifetime () : IClassicDesktopStyleApplicationLifetime option =
        match Application.Current.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> Some desktopLifetime
        | _ -> None

    let tryGetMainWindow () : Window option =
        tryGetDesktopLifetime ()
        |> Option.bind (fun lifetime -> lifetime.MainWindow |> Option.ofObj)

    let tryGetControl<'t when 't :> Visual> () : 't option =
        tryGetMainWindow ()
        |> Option.bind (fun window ->
            match window.GetVisualDescendants() |> Seq.tryFind (fun v -> v :? 't) with
            | Some control -> Some(control :?> 't)
            | None -> None)
