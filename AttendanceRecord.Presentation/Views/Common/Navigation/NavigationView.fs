namespace AttendanceRecord.Presentation.Views.Common.Navigation

open R3
open Material.Icons
open Avalonia.Media
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common

type PageItem =
    { Key: string
      Icon: MaterialIconKind
      Title: string
      View: unit -> Avalonia.Controls.Control }

type NavigationViewProps =
    { Pages: PageItem list
      InitialPageKey: string
      OnPageSelected: (string -> unit) option }

module NavigationView =
    open NXUI.Extensions
    open type NXUI.Builders

    let private createDrawer
        (ctx: NavigationContext)
        (pages: PageItem list)
        (isDrawerOpen: ReactiveProperty<bool>)
        =
        withLifecycle (fun disposables _ ->
            StackPanel()
                .Margin(12.0)
                .Children(
                    pages
                    |> List.map (fun item ->
                        let isSelected =
                            ctx.CurrentPageKey
                            |> R3.map (fun selectedKey -> selectedKey = item.Key)
                            |> R3.readonly None
                            |> R3.disposeWith disposables

                        AccentToggleButton.create isSelected
                        |> _.Content(MaterialIconLabel.create item.Icon item.Title)
                            .OnClickHandler(fun _ _ ->
                                ctx.NavigateTo item.Key |> ignore
                                isDrawerOpen.Value <- false)
                            .Margin(0, 5, 0, 5)
                            .Height(40.0)
                            .FontSize(16.0)
                            .HorizontalAlignmentStretch()
                            .HorizontalContentAlignmentLeft()
                            .Background(Brushes.Transparent)
                            .BorderBrush(Brushes.Transparent))
                    |> toChildren
                ))

    let create (props: NavigationViewProps) : Avalonia.Controls.Control =
        withLifecycle (fun disposables _ ->
            let ctx = createNavigationContext props.InitialPageKey disposables

            ctx.CurrentPageKey
            |> R3.subscribe (fun key ->
                match props.OnPageSelected with
                | Some callback -> callback key
                | None -> ())
            |> disposables.Add

            let isDrawerOpen = R3.property false |> R3.disposeWith disposables

            let pageTitle =
                ctx.CurrentPageKey
                |> R3.map (fun key ->
                    match props.Pages |> List.tryFind (fun page -> page.Key = key) with
                    | Some item -> item.Title
                    | None -> "Unknown")

            let pageContent =
                ctx.CurrentPageKey
                |> R3.map (fun key ->
                    match props.Pages |> List.tryFind (fun page -> page.Key = key) with
                    | Some item -> item.View()
                    | None -> ContentControl())

            let header =
                StackPanel()
                    .DockTop()
                    .OrientationHorizontal()
                    .Height(50.0)
                    .Margin(5.0)
                    .Children(
                        MaterialIconButton.create
                            { Kind = MaterialIconKind.Menu
                              OnClick = fun _ -> isDrawerOpen.Value <- true
                              FontSize = Some 18.0
                              Tooltip = Some "メニューを開く" }
                        |> _.Width(50.0).Height(50.0),
                        TextBlock()
                            .Text(pageTitle |> asBinding)
                            .FontSize(21.0)
                            .VerticalAlignmentCenter()
                            .Margin(20.0, 0.0, 0.0, 0.0)
                    )

            ctx
            |> Context.provide (
                SplitView()
                    .DisplayModeOverlay()
                    .UseLightDismissOverlayMode(true)
                    .OpenPaneLength(250.0)
                    .IsPaneOpen(isDrawerOpen |> asBinding)
                    .OnPaneClosedHandler(fun _ _ -> isDrawerOpen.Value <- false)
                    .Pane(createDrawer ctx props.Pages isDrawerOpen)
                    .Content(
                        DockPanel()
                            .LastChildFill(true)
                            .Children(header, ContentControl().Content(pageContent |> asBinding))
                    )
            ))
