namespace AttendanceRecord.Presentation.Utils

open Avalonia.Controls
open System.Runtime.CompilerServices
open Avalonia.Controls.Templates
open ObservableCollections

[<Extension>]
type __ItemsControlExtensions =
    [<Extension>]
    static member ItemTemplateFunc<'t, 'content, 'items
        when 'content :> Control and 'items :> ItemsControl>
        (control: 'items, template: 't -> 'content, ?supportsRecycling: bool)
        : 'items =
        control.ItemTemplate <-
            FuncDataTemplate<'t>(
                (fun data _ -> template data :> Control),
                supportsRecycling = defaultArg supportsRecycling true
            )

        control

    [<Extension>]
    static member TemplateFunc<'items when 'items :> ItemsControl>
        (control: 'items, builder: unit -> Control)
        : 'items =
        control.Template <- FuncControlTemplate(fun _ _ -> builder ())
        control

    [<Extension>]
    static member ItemsPanelFunc<'items, 'panel when 'items :> ItemsControl and 'panel :> Panel>
        (control: 'items, builder: unit -> 'panel)
        : 'items =
        control.ItemsPanel <- FuncTemplate<Panel>(fun () -> builder () :> Panel)
        control

    [<Extension>]
    static member ItemsSourceObservable<'items, 'T when 'items :> ItemsControl>
        (control: 'items, source: ObservableList<'T>)
        : 'items =
        control.ItemsSource <- source.ToNotifyCollectionChangedSlim()
        control
