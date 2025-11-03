namespace Avalonia.FuncUI.DSL

open System
open Avalonia
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.FuncUI.Types

type CjkTextBlock =
    /// Create a TextBlock and, on macOS, add extra top padding only when the Text contains CJK.
    static member create(attrs: list<IAttr<TextBlock>>) : IView<TextBlock> =
        let containsCjk (s: string) =
            s
            |> Seq.exists (fun ch ->
                let code = int ch

                code >= 0x3400 && code <= 0x9FFF
                || code >= 0x3040 && code <= 0x309F
                || code >= 0x30A0 && code <= 0x30FF
                || code >= 0xAC00 && code <= 0xD7AF
                || code >= 0xFF00 && code <= 0xFFEF)

        let needExtraPadding =
            OperatingSystem.IsMacOS()
            && match attrs |> ViewUtils.tryGetPropValue<TextBlock, string> "Text" with
               | Some text when containsCjk text -> true
               | _ -> false

        let finalAttrs =
            if needExtraPadding then
                let basePadding =
                    defaultArg (attrs |> ViewUtils.tryGetPropValue<TextBlock, Thickness> "Padding") (Thickness 0)

                let extra =
                    Thickness(basePadding.Left, basePadding.Top + 8.0, basePadding.Right, basePadding.Bottom)

                // Remove existing Padding attrs so the new one takes effect deterministically
                let attrsWithoutPadding =
                    attrs
                    |> List.filter (fun a -> a |> ViewUtils.tryGetProperty<TextBlock> "Padding" |> Option.isNone)

                attrsWithoutPadding @ [ TextBlock.padding extra ]
            else
                attrs

        TextBlock.create finalAttrs
