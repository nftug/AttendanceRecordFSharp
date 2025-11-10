namespace AttendanceRecord.Presentation.Views.Common

open NXUI.Extensions
open type NXUI.Builders
open Avalonia.Controls
open AttendanceRecord.Presentation.Utils
open R3
open Material.Icons
open Avalonia.Media
open System
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
    let private drawerView (props: DrawerViewProps<'key>) =
        withReactive (fun disposables _ ->
            StackPanel()
                .Margin(12.0)
                .Children(
                    props.Pages
                    |> Map.map (fun key item ->
                        let isSelected =
                            props.SelectedKey
                            |> R3.map (fun selectedKey -> selectedKey = key |> Nullable)
                            |> R3.readonly (Nullable false |> Some)
                            |> R3.disposeWith disposables

                        ToggleButton()
                            .Content(MaterialIconLabel.create item.Icon item.Title)
                            .OnIsCheckedChangedHandler(fun ctl _ -> ctl.IsChecked <- isSelected.CurrentValue)
                            .OnClickHandler(fun _ _ ->
                                props.SelectedKey.Value <- key
                                props.IsOpen.Value <- false)
                            .Margin(0, 5, 0, 5)
                            .Height(40.0)
                            .HorizontalAlignmentStretch()
                            .HorizontalContentAlignmentLeft()
                            .Background(Brushes.Transparent)
                            .BorderBrush(Brushes.Transparent)
                            .IsChecked(isSelected |> asBinding))
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
                        Button()
                            .Content(MaterialIcon.create MaterialIconKind.Menu)
                            .Width(50.0)
                            .Height(50.0)
                            .FontSize(18.0)
                            .Background(Brushes.Transparent)
                            .BorderBrush(Brushes.Transparent)
                            .OnClickHandler(fun _ _ -> isDrawerOpen.Value <- true),
                        TextBlock()
                            .Text(pageTitle |> asBinding)
                            .FontSize(21.0)
                            .VerticalAlignmentCenter()
                            .Margin(20.0, 0.0, 0.0, 0.0)
                    )

            SplitView()
                .DisplayModeOverlay()
                .OpenPaneLength(250.0)
                .IsPaneOpen(isDrawerOpen |> asBinding)
                .OnPaneClosedHandler(fun _ _ -> isDrawerOpen.Value <- false)
                .Pane(
                    drawerView
                        { IsOpen = isDrawerOpen
                          Pages = props.Pages
                          SelectedKey = selectedPageKey }
                )
                .Content(
                    DockPanel()
                        .LastChildFill(true)
                        .Children(header, ContentControl().Content(pageContent |> asBinding))
                ))
