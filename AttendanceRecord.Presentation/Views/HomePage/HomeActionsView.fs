namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open R3
open System
open System.Threading.Tasks
open System.Threading
open Material.Icons
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Application.Dtos.Responses

type HomeActionsViewProps =
    { Status: Observable<CurrentStatusDto>
      OnToggleWork: unit -> CancellationToken -> Task<Result<CurrentStatusDto, string>>
      OnToggleRest: unit -> CancellationToken -> Task<Result<CurrentStatusDto, string>> }

module HomeActionsView =
    let create (props: HomeActionsViewProps) : Avalonia.Controls.Control =
        withReactive (fun disposables _ ->
            let hooks =
                useHomeActionsHooks
                    { Status = props.Status
                      OnToggleWork = props.OnToggleWork
                      OnToggleRest = props.OnToggleRest }
                    disposables

            Grid()
                .RowDefinitions("Auto")
                .ColumnDefinitions("*,*")
                .Children(
                    ToggleButton()
                        .Column(0)
                        .Content(
                            hooks.WorkButtonLabel
                            |> MaterialIconLabel.createObservable MaterialIconKind.Work
                        )
                        .OnClickHandler(fun _ _ -> hooks.HandleClickToggleWork())
                        .IsEnabled(hooks.WorkToggleEnabled |> asBinding)
                        .IsChecked(hooks.Status |> R3.map (fun s -> s.IsWorking |> Nullable) |> asBinding)
                        .OnIsCheckedChangedHandler(fun ctl _ -> ctl.IsChecked <- hooks.Status.CurrentValue.IsWorking)
                        .Height(46.0)
                        .FontSize(16.0)
                        .HorizontalAlignmentStretch()
                        .Margin(0, 0, 10.0, 0),
                    ToggleButton()
                        .Column(1)
                        .Content(
                            hooks.RestButtonLabel
                            |> MaterialIconLabel.createObservable MaterialIconKind.Coffee
                        )
                        .OnClickHandler(fun _ _ -> hooks.HandleClickToggleRest())
                        .IsEnabled(hooks.RestToggleEnabled |> asBinding)
                        .IsChecked(hooks.Status |> R3.map (fun s -> s.IsResting |> Nullable) |> asBinding)
                        .OnIsCheckedChangedHandler(fun ctl _ -> ctl.IsChecked <- hooks.Status.CurrentValue.IsResting)
                        .Height(46.0)
                        .FontSize(16.0)
                        .HorizontalAlignmentStretch()
                ))
