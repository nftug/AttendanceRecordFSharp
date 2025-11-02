namespace Avalonia.FuncUI.DSL

open Material.Icons.Avalonia
open Material.Icons
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.Builder

[<AutoOpen>]
module MaterialIcon =
    let create (attrs: IAttr<MaterialIcon> list) : IView<MaterialIcon> = ViewBuilder.Create<MaterialIcon> attrs

    type MaterialIcon with

        static member kind<'t when 't :> MaterialIcon>(value: MaterialIconKind) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<MaterialIconKind>(MaterialIcon.KindProperty, value, ValueNone)

module MaterialIconLabel =
    open Avalonia.Controls
    open Avalonia.Layout

    let create (icon: MaterialIconKind) (children: IView list) : IView<StackPanel> =
        StackPanel.create
            [ StackPanel.orientation Orientation.Horizontal
              StackPanel.spacing 8.0
              StackPanel.children [ MaterialIcon.create [ MaterialIcon.kind icon ]; yield! children ] ]
