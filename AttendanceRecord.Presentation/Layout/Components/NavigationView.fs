namespace AttendanceRecord.Presentation.Layout.Components

open Avalonia
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Media
open Material.Icons
open Material.Icons.Avalonia
open Avalonia.FuncUI.Types
open Avalonia.Layout
open Avalonia.Input

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
                            [ TextBlock.create
                                  [ TextBlock.text item.Title
                                    TextBlock.fontSize 16.0
                                    TextBlock.verticalAlignment VerticalAlignment.Center ] ]
                    | None -> TextBlock.create [ TextBlock.text "Unknown" ]

                let handleSelectedItemChanged (args: obj) =
                    isOpen.Set false
                    pageKey.Set(args :?> 'key)

                DockPanel.create
                    [ DockPanel.lastChildFill true
                      DockPanel.margin 12.0
                      DockPanel.children
                          [ Button.create
                                [ DockPanel.dock Dock.Top
                                  Button.content (MaterialIcon.create [ MaterialIcon.kind MaterialIconKind.Menu ])
                                  Button.background (null :> Brush)
                                  Button.onClick (fun _ -> isOpen.Set(not isOpen.Current))
                                  Button.width 35.0
                                  Button.height 35.0
                                  Button.fontSize 18.0
                                  Button.cursor (Cursor.Parse "Hand") ]
                            ListBox.create
                                [ DockPanel.dock Dock.Bottom
                                  ListBox.isVisible isOpen.Current
                                  ListBox.margin (Thickness(0.0, 10.0, 0.0, 0.0))
                                  ListBox.selectedItem pageKey.Current
                                  ListBox.onSelectedItemChanged handleSelectedItemChanged
                                  ListBox.dataItems (props.PageItems |> Map.keys |> List.ofSeq)
                                  ListBox.itemTemplate (DataTemplateView<'key>.create (fun item -> createMenuItem item))
                                  ListBox.background (null :> Brush) ] ] ]
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

                SplitView.create
                    [ SplitView.displayMode SplitViewDisplayMode.CompactInline
                      SplitView.openPaneLength 250.0
                      SplitView.compactPaneLengthProperty 55.0
                      SplitView.isPaneOpen isOpen.Current
                      SplitView.pane (
                          drawerView
                              { IsOpen = isOpen
                                PageItems = props.PageItems
                                PageKey = pageKey }
                      )
                      SplitView.content (
                          DockPanel.create
                              [ DockPanel.lastChildFill true
                                DockPanel.children
                                    [ TextBlock.create
                                          [ DockPanel.dock Dock.Top
                                            TextBlock.text pageTitle
                                            TextBlock.fontSize 22.0
                                            TextBlock.horizontalAlignment HorizontalAlignment.Left
                                            TextBlock.margin (Thickness(20.0, 20.0, 0.0, 10.0)) ]
                                      match props.PageItems |> Map.tryFind pageKey.Current with
                                      | Some item -> item.Content
                                      | None -> TextBlock.create [ TextBlock.text "Page not found" ] ] ]
                      ) ]
        )
