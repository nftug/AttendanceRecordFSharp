namespace AttendanceRecord.Presentation.Views.Common.Navigation

open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open FluentAvalonia.UI.Controls
open type NXUI.Builders
open NXUI.Extensions

type NavigationViewProps =
   { MenuItems: NavigationViewItem list
     FooterMenuItems: NavigationViewItem list }

module NavigationView =
   type private NavigationFactory() =
      interface INavigationPageFactory with
         member _.GetPage _ = null

         member _.GetPageFromObject(target: obj) : Avalonia.Controls.Control =
            target :?> Avalonia.Controls.Control

   let private findNavigationItemByRoute
      (props: NavigationViewProps)
      (route: Route)
      : NavigationViewItem option =
      props.MenuItems @ props.FooterMenuItems
      |> List.tryFind (fun item -> (item.Tag :?> string) = route.Path)

   let private createContent (ctx: NavigationContext) (props: NavigationViewProps) =
      withLifecycle (fun disposables _ ->
         let pageTitle =
            ctx.CurrentRoute
            |> R3.map (fun route ->
               findNavigationItemByRoute props route
               |> Option.map _.Content
               |> Option.defaultValue "Unknown")

         let frame =
            Frame(
               NavigationPageFactory = NavigationFactory(),
               IsNavigationStackEnabled = false,
               CacheSize = 0
            )

         ctx.CurrentRoute
         |> R3.subscribe (fun r -> frame.NavigateFromObject(r.ViewFn()) |> ignore)
         |> disposables.Add

         DockPanel()
            .LastChildFill(true)
            .Children(
               TextBlock()
                  .Text(pageTitle |> asBinding)
                  .FontSize(22.0)
                  .Margin(20.0, 20.0, 0.0, 10.0)
                  .DockTop(),
               frame
            ))

   let create (props: NavigationViewProps) =
      withLifecycle (fun disposables self ->
         let ctx, _ = Context.require<NavigationContext> self

         let navigation =
            NavigationView(
               PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact,
               IsSettingsVisible = false,
               MenuItemsSource = (props.MenuItems |> List.toArray),
               FooterMenuItemsSource = (props.FooterMenuItems |> List.toArray),
               Content = createContent ctx props
            )

         ctx.CurrentRoute
         |> R3.map (findNavigationItemByRoute props)
         |> R3.subscribe (Option.iter (fun item -> navigation.SelectedItem <- item))
         |> disposables.Add

         navigation.SelectionChanged.Add(fun args ->
            args.SelectedItem
            |> Option.ofObj
            |> Option.iter (fun v ->
               let navItem = v :?> NavigationViewItem
               let path = navItem.Tag :?> string

               (fun () -> ctx.NavigateTo path)
               |> thenDo (fun result ->
                  if not result then
                     // Revert selection if navigation was cancelled
                     navigation.SelectedItem <-
                        ctx.CurrentRoute.CurrentValue
                        |> findNavigationItemByRoute props
                        |> Option.defaultValue navItem)
               |> ignore))

         navigation)
