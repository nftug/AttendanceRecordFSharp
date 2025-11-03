namespace AttendanceRecord.Presentation.Layout.Components

open Avalonia
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Media
open Material.Icons
open Avalonia.FuncUI.Types
open Avalonia.Layout
open Avalonia.Controls.Primitives

type PageItem<'key when 'key: comparison> =
    { Icon: MaterialIconKind
      Title: string
      Content: IView }

type NavigationViewProps<'key when 'key: comparison> =
    { PageItems: Map<'key, PageItem<'key>>
      PageKey: IWritable<'key> }

type private DrawerViewProps<'key when 'key: comparison> =
    { IsOpen: IWritable<bool>
      PageItems: Map<'key, PageItem<'key>>
      PageKey: IWritable<'key> }

module NavigationView =
    let private drawerView (props: DrawerViewProps<'key>) =
        Component.create (
            "DrawerView",
            fun ctx ->
                let isOpen = props.IsOpen |> ctx.usePassed
                let pageKey = props.PageKey |> ctx.usePassed

                let createMenuItem (key: 'key) : IView =
                    match props.PageItems |> Map.tryFind key with
                    | Some item ->
                        MaterialIconLabel.create
                            item.Icon
                            [ CjkTextBlock.create
                                  [ TextBlock.text item.Title
                                    TextBlock.fontSize 16.0
                                    TextBlock.verticalAlignment VerticalAlignment.Center ] ]
                    | None -> CjkTextBlock.create [ TextBlock.text "Unknown" ]

                DockPanel.create
                    [ DockPanel.lastChildFill true
                      DockPanel.margin 12.0
                      DockPanel.children
                          [ StackPanel.create
                                [ StackPanel.children (
                                      props.PageItems
                                      |> Map.keys
                                      |> Seq.map (fun key ->
                                          ToggleButton.create
                                              [ ToggleButton.content (createMenuItem key)
                                                ToggleButton.isChecked (pageKey.Current = key)
                                                ToggleButton.height 40.0
                                                ToggleButton.margin (Thickness(0.0, 5.0, 0.0, 5.0))
                                                ToggleButton.horizontalContentAlignment HorizontalAlignment.Left
                                                ToggleButton.horizontalAlignment HorizontalAlignment.Stretch
                                                ToggleButton.background Brushes.Transparent
                                                ToggleButton.borderBrush Brushes.Transparent
                                                ToggleButton.onChecked (fun _ ->
                                                    isOpen.Set false
                                                    pageKey.Set key) ]
                                          :> IView)
                                      |> List.ofSeq
                                  ) ] ] ]
        )

    let view (props: NavigationViewProps<'key>) =
        Component.create (
            "NavigationView",
            fun ctx ->
                let isOpen = ctx.useState false
                let pageKey = props.PageKey |> ctx.usePassed

                let pageTitle =
                    match props.PageItems |> Map.tryFind pageKey.Current with
                    | Some item -> item.Title
                    | None -> "Unknown"

                let pageContent =
                    match props.PageItems |> Map.tryFind pageKey.Current with
                    | Some item -> item.Content
                    | None -> CjkTextBlock.create [ TextBlock.text "Page not found" ]

                let content =
                    DockPanel.create
                        [ DockPanel.lastChildFill true
                          DockPanel.children
                              [ StackPanel.create
                                    [ DockPanel.dock Dock.Top
                                      StackPanel.orientation Orientation.Horizontal
                                      StackPanel.height 50.0
                                      StackPanel.margin 5.0
                                      StackPanel.children
                                          [ Button.create
                                                [ Button.content (MaterialIconLabel.create MaterialIconKind.Menu [])
                                                  Button.width 50.0
                                                  Button.height 50.0
                                                  Button.fontSize 18.0
                                                  Button.background Brushes.Transparent
                                                  Button.borderBrush Brushes.Transparent
                                                  Button.onClick (fun _ -> isOpen.Set true) ]
                                            CjkTextBlock.create
                                                [ TextBlock.text pageTitle
                                                  TextBlock.fontSize 21.0
                                                  TextBlock.verticalAlignment VerticalAlignment.Center
                                                  TextBlock.margin (Thickness(20.0, 0.0, 0.0, 0.0)) ] ] ]
                                pageContent ] ]

                SplitView.create
                    [ SplitView.displayMode SplitViewDisplayMode.Overlay
                      SplitView.openPaneLength 250.0
                      SplitView.isPaneOpen isOpen.Current
                      SplitView.onPaneClosed (fun _ -> isOpen.Set false)
                      SplitView.pane (
                          drawerView
                              { IsOpen = isOpen
                                PageItems = props.PageItems
                                PageKey = pageKey }
                      )
                      SplitView.content content ]
        )
