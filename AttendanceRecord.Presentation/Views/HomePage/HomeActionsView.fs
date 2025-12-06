namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open R3
open FluentAvalonia.UI.Controls
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HomePage.Context

module HomeActionsView =
   let create () : Avalonia.Controls.Control =
      withLifecycle (fun _ self ->
         let ctx, ctxDisposables = Context.require<HomePageContext> self
         let hooks = useHomeActionsHooks ctx ctxDisposables

         Grid()
            .RowDefinitions("Auto")
            .ColumnDefinitions("*,*")
            .Children(
               AccentToggleButton.create (hooks.Status |> R3.map _.IsWorking)
               |> _.Column(0)
                  .Content(
                     SymbolIconLabel.create
                        { Symbol =
                           hooks.Status
                           |> R3.map _.IsActive
                           |> R3.map (function
                              | true -> Symbol.Stop
                              | false -> Symbol.Play)
                          Label = hooks.WorkButtonLabel
                          Spacing = None |> R3.ret }
                  )
                  .OnClickHandler(fun _ _ -> hooks.HandleClickToggleWork())
                  .IsEnabled(hooks.WorkToggleEnabled |> asBinding)
                  .Height(46.0)
                  .FontSize(16.0)
                  .HorizontalAlignmentStretch()
                  .Margin(0, 0, 10.0, 0),
               AccentToggleButton.create (hooks.Status |> R3.map _.IsResting)
               |> _.Column(1)
                  .Content(
                     SymbolIconLabel.create
                        { Symbol = Symbol.Pause |> R3.ret
                          Label = hooks.RestButtonLabel
                          Spacing = None |> R3.ret }
                  )
                  .OnClickHandler(fun _ _ -> hooks.HandleClickToggleRest())
                  .IsEnabled(hooks.RestToggleEnabled |> asBinding)
                  .Height(46.0)
                  .FontSize(16.0)
                  .HorizontalAlignmentStretch()
            ))
