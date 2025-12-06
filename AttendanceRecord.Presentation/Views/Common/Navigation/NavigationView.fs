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

         let frame = Frame(NavigationPageFactory = NavigationFactory())

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
         |> R3.subscribe (fun route ->
            findNavigationItemByRoute props route
            |> Option.iter (fun item -> navigation.SelectedItem <- item))
         |> disposables.Add

         navigation.SelectionChanged.Add(fun args ->
            args.SelectedItem
            |> Option.ofObj
            |> Option.iter (fun v ->
               let navItem = v :?> NavigationViewItem

               // Sync selected item with current route
               navigation.SelectedItem <-
                  ctx.CurrentRoute.CurrentValue
                  |> findNavigationItemByRoute props
                  |> Option.defaultValue navItem

               let path = navItem.Tag :?> string
               ctx.NavigateTo path |> ignore))

         navigation)
