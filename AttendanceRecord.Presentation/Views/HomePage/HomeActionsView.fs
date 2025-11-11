namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open R3
open System
open Material.Icons
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils

module HomeActionsView =
    let create () : Avalonia.Controls.Control =
        withReactive (fun disposables self ->
            let hooks = useHomeActionsHooks self disposables

            Grid()
                .RowDefinitions("Auto")
                .ColumnDefinitions("*,*")
                .Children(
                    ToggleButton()
                        .Column(0)
                        .Content(
                            hooks.WorkButtonLabel
                            |> toView (MaterialIconLabel.create MaterialIconKind.Work)
                        )
                        .OnClickHandler(fun _ _ -> hooks.HandleClickToggleWork())
                        .IsEnabled(hooks.WorkToggleEnabled |> asBinding)
                        .IsChecked(
                            hooks.Status |> R3.map (fun s -> s.IsWorking |> Nullable) |> asBinding
                        )
                        .OnIsCheckedChangedHandler(fun ctl _ ->
                            ctl.IsChecked <- hooks.Status.CurrentValue.IsWorking)
                        .Height(46.0)
                        .FontSize(16.0)
                        .HorizontalAlignmentStretch()
                        .Margin(0, 0, 10.0, 0),
                    ToggleButton()
                        .Column(1)
                        .Content(
                            hooks.RestButtonLabel
                            |> toView (MaterialIconLabel.create MaterialIconKind.Coffee)
                        )
                        .OnClickHandler(fun _ _ -> hooks.HandleClickToggleRest())
                        .IsEnabled(hooks.RestToggleEnabled |> asBinding)
                        .IsChecked(
                            hooks.Status |> R3.map (fun s -> s.IsResting |> Nullable) |> asBinding
                        )
                        .OnIsCheckedChangedHandler(fun ctl _ ->
                            ctl.IsChecked <- hooks.Status.CurrentValue.IsResting)
                        .Height(46.0)
                        .FontSize(16.0)
                        .HorizontalAlignmentStretch()
                ))
