namespace Avalonia

open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes

module ApplicationUtils =
    let tryGetDesktopLifetime () : IClassicDesktopStyleApplicationLifetime option =
        match Application.Current.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> Some desktopLifetime
        | _ -> None

    let tryGetMainWindow () : Window option =
        tryGetDesktopLifetime ()
        |> Option.bind (fun lifetime -> lifetime.MainWindow |> Option.ofObj)
