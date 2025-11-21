namespace AttendanceRecord.Presentation.Views.Common

open NXUI.Extensions
open Avalonia.Controls
open AttendanceRecord.Presentation.Utils
open R3
open Material.Icons
open Avalonia.Media
open AttendanceRecord.Shared

type PageItem<'key when 'key: comparison> =
    { Icon: MaterialIconKind
      Title: string
      View: Control }

type NavigationViewProps<'key when 'key: comparison> =
    { Pages: Map<'key, PageItem<'key>>
      InitialPageKey: 'key
      OnPageSelected: ('key -> unit) option }

type private DrawerViewProps<'key when 'key: comparison> =
    { IsOpen: ReactiveProperty<bool>
      Pages: Map<'key, PageItem<'key>>
      SelectedKey: ReactiveProperty<'key> }

module NavigationView =
    let private createDrawer (props: DrawerViewProps<'key>) =
        withReactive (fun disposables _ ->
            StackPanel()
                .Margin(12.0)
                .Children(
                    props.Pages
                    |> Map.map (fun key item ->
                        let isSelected =
                            props.SelectedKey
                            |> R3.map (fun selectedKey -> selectedKey = key)
                            |> R3.readonly None
                            |> R3.disposeWith disposables

                        (AccentToggleButton.create isSelected)
                            .Content(MaterialIconLabel.create item.Icon item.Title)
                            .OnClickHandler(fun _ _ ->
                                props.SelectedKey.Value <- key
                                props.IsOpen.Value <- false)
                            .Margin(0, 5, 0, 5)
                            .Height(40.0)
                            .FontSize(16.0)
                            .HorizontalAlignmentStretch()
                            .HorizontalContentAlignmentLeft()
                            .Background(Brushes.Transparent)
                            .BorderBrush(Brushes.Transparent))
                    |> Map.toList
                    |> List.map snd
                    |> toChildren
                ))

    let create (props: NavigationViewProps<'key>) : Control =
        withReactive (fun disposables _ ->
            let isDrawerOpen = R3.property false |> R3.disposeWith disposables

            let selectedPageKey = R3.property props.InitialPageKey |> R3.disposeWith disposables

            selectedPageKey
            |> R3.subscribe (fun key ->
                match props.OnPageSelected with
                | Some callback -> callback key
                | None -> ())
            |> disposables.Add

            let pageTitle =
                selectedPageKey
                |> R3.map (fun key ->
                    match props.Pages |> Map.tryFind key with
                    | Some item -> item.Title
                    | None -> "Unknown")

            let pageContent =
                selectedPageKey
                |> R3.map (fun key ->
                    match props.Pages |> Map.tryFind key with
                    | Some item -> item.View
                    | None -> ContentControl())

            let header =
                StackPanel()
                    .DockTop()
                    .OrientationHorizontal()
                    .Height(50.0)
                    .Margin(5.0)
                    .Children(
                        (MaterialIconButton.create
                            { Kind = MaterialIconKind.Menu
                              OnClick = fun _ -> isDrawerOpen.Value <- true
                              FontSize = Some 18.0
                              Tooltip = Some "メニューを開く" })
                            .Width(50.0)
                            .Height(50.0),
                        TextBlock()
                            .Text(pageTitle |> asBinding)
                            .FontSize(21.0)
                            .VerticalAlignmentCenter()
                            .Margin(20.0, 0.0, 0.0, 0.0)
                    )

            SplitView()
                .DisplayModeOverlay()
                .UseLightDismissOverlayMode(true)
                .OpenPaneLength(250.0)
                .IsPaneOpen(isDrawerOpen |> asBinding)
                .OnPaneClosedHandler(fun _ _ -> isDrawerOpen.Value <- false)
                .Pane(
                    createDrawer
                        { IsOpen = isDrawerOpen
                          Pages = props.Pages
                          SelectedKey = selectedPageKey }
                )
                .Content(
                    DockPanel()
                        .LastChildFill(true)
                        .Children(header, ContentControl().Content(pageContent |> asBinding))
                ))
