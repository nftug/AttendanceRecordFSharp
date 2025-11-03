namespace Avalonia.FuncUI.DSL

open DialogHostAvalonia
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types

[<AutoOpen>]
module DialogHost =
    let create (attrs: IAttr<DialogHost> list) : IView<DialogHost> = ViewBuilder.Create<DialogHost> attrs
