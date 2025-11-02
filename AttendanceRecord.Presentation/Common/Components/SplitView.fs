namespace Avalonia.FuncUI.DSL

open Avalonia.Controls
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.Builder

module SplitView =
    let create (attrs: IAttr<SplitView> list) : IView<SplitView> = ViewBuilder.Create<SplitView> attrs

    type SplitView with

        static member isPaneOpen<'t when 't :> SplitView>(value: bool) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<bool>(SplitView.IsPaneOpenProperty, value, ValueNone)

        static member displayMode<'t when 't :> SplitView>(value: SplitViewDisplayMode) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<SplitViewDisplayMode>(SplitView.DisplayModeProperty, value, ValueNone)

        static member openPaneLength<'t when 't :> SplitView>(value: float) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<float>(SplitView.OpenPaneLengthProperty, value, ValueNone)

        static member pane<'t when 't :> SplitView>(value: option<IView>) : IAttr<'t> =
            AttrBuilder<'t>.CreateContentSingle(SplitView.PaneProperty, value)

        static member content<'t when 't :> SplitView>(value: option<IView>) : IAttr<'t> =
            AttrBuilder<'t>.CreateContentSingle(SplitView.ContentProperty, value)
