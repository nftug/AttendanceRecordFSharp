namespace Avalonia.FuncUI

open System
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.FuncUI.Types

type CjkTextBlock =
    static member create(attr: list<IAttr<TextBlock>>) : IView<TextBlock> =
        let padding =
            if OperatingSystem.IsMacOS() then
                Avalonia.Thickness(0, 8.0, 0, 0)
            else
                Avalonia.Thickness 0

        TextBlock.create [ TextBlock.padding padding; yield! attr ]
