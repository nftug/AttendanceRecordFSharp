namespace AttendanceRecord.Presentation.Utils

open Avalonia.Controls
open System.Runtime.CompilerServices
open Avalonia.Controls.Templates

[<Extension>]
type __ItemsControlExtensions =
    [<Extension>]
    static member ItemTemplate
        (
            control: ItemsControl,
            template: 'obs -> Control,
            ?supportsRecycling: bool
        ) : ItemsControl =
        let template =
            FuncDataTemplate<'obs>(
                (fun data _ -> template data),
                supportsRecycling = defaultArg supportsRecycling true
            )

        control.ItemTemplate <- template
        control
