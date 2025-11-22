namespace AttendanceRecord.Presentation.Utils

open Avalonia.Controls
open System.Runtime.CompilerServices
open Avalonia.Controls.Templates

[<Extension>]
type __ItemsControlExtensions =
    [<Extension>]
    static member ItemTemplate<'obs, 'control when 'control :> Control>
        (control: ItemsControl, template: 'obs -> 'control, ?supportsRecycling: bool)
        : ItemsControl =
        let template =
            FuncDataTemplate<'obs>(
                (fun data _ -> template data :> Control),
                supportsRecycling = defaultArg supportsRecycling true
            )

        control.ItemTemplate <- template
        control
