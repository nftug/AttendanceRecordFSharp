namespace AttendanceRecord.Presentation.Views.Common.Navigation

open R3
open Material.Icons
open Avalonia.Media
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common

type DrawerMenuItem =
    { Path: string
      Icon: MaterialIconKind
      Title: string }

module NavigationView =
    open NXUI.Extensions
    open type NXUI.Builders

    let private createDrawer
        (ctx: NavigationContext)
        (pages: DrawerMenuItem list)
        (closeDrawer: unit -> unit)
        =
        ItemsControl()
            .HorizontalAlignmentStretch()
            .ItemsSource(pages |> List.toArray)
            .TemplateFunc(fun _ -> ScrollViewer().Margin(15.0).Content(ItemsPresenter()))
            .ItemTemplateFunc(fun (item: DrawerMenuItem) ->
                let isSelected = ctx.CurrentRoute |> R3.map (fun r -> r.Path = item.Path)

                (AccentToggleButton.create isSelected)
                    .Content(
                        MaterialIconLabel.create
                            { Kind = item.Icon |> R3.ret
                              Label = item.Title |> R3.ret
                              Spacing = Some 12.0 |> R3.ret }
                    )
                    .OnClickHandler(fun _ _ ->
                        ctx.NavigateTo item.Path |> ignore
                        closeDrawer ())
                    .HorizontalAlignmentStretch()
                    .HorizontalContentAlignmentLeft()
                    .Background(Brushes.Transparent)
                    .BorderBrush(Brushes.Transparent)
                    .CornerRadius(0.0)
                    .Height(50.0)
                    .FontSize(16.0))

    let create (menuItems: DrawerMenuItem list) : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<NavigationContext> self

            let isDrawerOpen = R3.property false |> R3.disposeWith disposables

            let pageTitle =
                ctx.CurrentRoute
                |> R3.map (fun r ->
                    match menuItems |> List.tryFind (fun page -> page.Path = r.Path) with
                    | Some item -> item.Title
                    | None -> "Unknown")

            let buildHeader () =
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
                .Pane(createDrawer ctx menuItems (fun () -> isDrawerOpen.Value <- false))
                .Content(
                    DockPanel()
                        .LastChildFill(true)
                        .Children(
                            buildHeader (),
                            ctx.CurrentRoute |> toView (fun _ _ r -> r.ViewFn())
                        )
                ))
