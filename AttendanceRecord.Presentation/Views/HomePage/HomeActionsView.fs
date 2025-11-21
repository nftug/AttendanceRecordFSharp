namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open R3
open Material.Icons
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common

module HomeActionsView =
    let create () : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = HomePageContextProvider.require self
            let hooks = useHomeActionsHooks ctx disposables

            let isWorking =
                hooks.Status
                |> R3.map _.IsWorking
                |> R3.readonly None
                |> R3.disposeWith disposables

            let isResting =
                hooks.Status
                |> R3.map _.IsResting
                |> R3.readonly None
                |> R3.disposeWith disposables

            Grid()
                .RowDefinitions("Auto")
                .ColumnDefinitions("*,*")
                .Children(
                    AccentToggleButton.create isWorking
                    |> _.Column(0)
                        .Content(
                            hooks.WorkButtonLabel
                            |> toView (fun _ _ -> MaterialIconLabel.create MaterialIconKind.Work)
                        )
                        .OnClickHandler(fun _ _ -> hooks.HandleClickToggleWork())
                        .IsEnabled(hooks.WorkToggleEnabled |> asBinding)
                        .Height(46.0)
                        .FontSize(16.0)
                        .HorizontalAlignmentStretch()
                        .Margin(0, 0, 10.0, 0),
                    AccentToggleButton.create isResting
                    |> _.Column(1)
                        .Content(
                            hooks.RestButtonLabel
                            |> toView (fun _ _ -> MaterialIconLabel.create MaterialIconKind.Coffee)
                        )
                        .OnClickHandler(fun _ _ -> hooks.HandleClickToggleRest())
                        .IsEnabled(hooks.RestToggleEnabled |> asBinding)
                        .Height(46.0)
                        .FontSize(16.0)
                        .HorizontalAlignmentStretch()
                ))
