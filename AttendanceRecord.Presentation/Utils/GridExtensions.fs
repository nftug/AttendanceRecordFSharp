namespace AttendanceRecord.Presentation.Utils

open System
open Avalonia.Controls
open System.Runtime.CompilerServices

[<Extension>]
type __GridExtensions =
    [<Extension>]
    static member RowDefinitions(grid: Grid, definitions: string) : Grid =
        definitions.Split(',', StringSplitOptions.RemoveEmptyEntries)
        |> Seq.iter (fun def -> grid.RowDefinitions.Add(RowDefinition(GridLength.Parse def)))

        grid

    [<Extension>]
    static member ColumnDefinitions(grid: Grid, definitions: string) : Grid =
        definitions.Split(',', StringSplitOptions.RemoveEmptyEntries)
        |> Seq.iter (fun def -> grid.ColumnDefinitions.Add(ColumnDefinition(GridLength.Parse def)))

        grid
