namespace Avalonia.FuncUI.DSL

module ViewUtils =
    open Avalonia
    open Avalonia.FuncUI
    open Avalonia.FuncUI.Types

    let tryGetProperty<'T when 'T :> AvaloniaObject> (propName: string) (attr: IAttr<'T>) : Property option =
        match attr with
        | :? Types.Attr<'T> as attr ->
            match attr with
            | Property prop ->
                match prop.Accessor with
                | AvaloniaProperty ap when ap.Name = propName -> Some prop
                | _ -> None
            | _ -> None
        | _ -> None

    let tryGetPropValue<'T, 'V when 'T :> AvaloniaObject> (propName: string) (attrs: list<IAttr<'T>>) : 'V option =
        attrs
        |> List.tryPick (tryGetProperty<'T> propName)
        |> Option.bind (fun prop ->
            match prop.Value with
            | :? 'V as v -> Some v
            | _ -> None)
