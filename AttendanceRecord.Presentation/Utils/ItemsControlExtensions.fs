namespace AttendanceRecord.Presentation.Utils

open Avalonia.Controls
open System.Runtime.CompilerServices
open Avalonia.Controls.Templates
open Avalonia.Controls.Primitives

[<Extension>]
type __ItemsControlExtensions =
    [<Extension>]
    static member ItemTemplateFunc<'obs, 'control when 'control :> Control>
        (control: ItemsControl, template: 'obs -> 'control, ?supportsRecycling: bool)
        : ItemsControl =
        control.ItemTemplate <-
            FuncDataTemplate<'obs>(
                (fun data _ -> template data :> Control),
                supportsRecycling = defaultArg supportsRecycling true
            )

        control

    [<Extension>]
    static member TemplateFunc
        (control: ItemsControl, builder: unit -> TemplatedControl)
        : ItemsControl =
        control.Template <- FuncControlTemplate(fun _ _ -> builder ())
        control

    [<Extension>]
    static member ItemsPanelFunc<'panel when 'panel :> Panel>
        (control: ItemsControl, builder: unit -> 'panel)
        : ItemsControl =
        control.ItemsPanel <- FuncTemplate<Panel>(fun () -> builder () :> Panel)
        control
