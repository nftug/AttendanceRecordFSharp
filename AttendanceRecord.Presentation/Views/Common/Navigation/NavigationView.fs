namespace AttendanceRecord.Presentation.Views.Common.Navigation

open R3
open Material.Icons
open Avalonia.Media
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common

type DrawerMenuItems =
    { Path: string
      Icon: MaterialIconKind
      Title: string }

module NavigationView =
    open NXUI.Extensions
    open type NXUI.Builders

    let private createDrawer (pages: DrawerMenuItems list) (closeDrawer: unit -> unit) =
        withLifecycle (fun _ self ->
            let ctx, _ = Context.require<NavigationContext> self

            StackPanel()
                .Margin(12.0)
                .Children(
                    pages
                    |> List.map (fun item ->
                        let isSelected = ctx.CurrentRoute |> R3.map (fun r -> r.Path = item.Path)

                        AccentToggleButton.create isSelected
                        |> _.Margin(0, 5, 0, 5)
                            .Content(
                                MaterialIconLabel.create
                                    { Kind = item.Icon |> R3.ret
                                      Label = item.Title |> R3.ret
                                      Spacing = None |> R3.ret }
                            )
                            .OnClickHandler(fun _ _ ->
                                ctx.NavigateTo item.Path |> ignore
                                closeDrawer ())
                            .Height(40.0)
                            .FontSize(16.0)
                            .HorizontalAlignmentStretch()
                            .HorizontalContentAlignmentLeft()
                            .Background(Brushes.Transparent)
                            .BorderBrush(Brushes.Transparent))
                    |> toChildren
                ))

    let create (menuItems: DrawerMenuItems list) : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<NavigationContext> self

            let isDrawerOpen = R3.property false |> R3.disposeWith disposables

            let pageTitle =
                ctx.CurrentRoute
                |> R3.map (fun r ->
                    match menuItems |> List.tryFind (fun page -> page.Path = r.Path) with
                    | Some item -> item.Title
                    | None -> "Unknown")

            let header =
                StackPanel()
                    .DockTop()
                    .OrientationHorizontal()
                    .Height(50.0)
                    .Margin(5.0)
                    .Children(
                        MaterialIconButton.create
                            { Kind = MaterialIconKind.Menu |> R3.ret
                              OnClick = fun _ -> isDrawerOpen.Value <- true
                              FontSize = Some 18.0 |> R3.ret
                              Tooltip = None |> R3.ret }
                        |> _.Width(50.0).Height(50.0),
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
                .Pane(createDrawer menuItems (fun () -> isDrawerOpen.Value <- false))
                .Content(
                    DockPanel()
                        .LastChildFill(true)
                        .Children(
                            header,
                            ContentControl()
                                .Content(ctx.CurrentRoute |> R3.map _.ViewFn() |> asBinding)
                        )
                ))
