namespace AttendanceRecord.Presentation.Features.HomePage.Components

open FsToolkit.ErrorHandling
open Avalonia
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Controls.Primitives
open AttendanceRecord.Application.Dtos.Responses

type HomeActionsViewProps =
    { Status: IReadable<CurrentStatusDto>
      OnToggleWork: unit -> TaskResult<CurrentStatusDto, string>
      OnToggleRest: unit -> TaskResult<CurrentStatusDto, string> }

module HomeActionsView =
    let view (props: HomeActionsViewProps) =
        Component.create (
            "HomeActionsView",
            fun ctx ->
                let status = props.Status |> ctx.usePassedRead
                let toggleWorkMutation = ctx.useMutation props.OnToggleWork
                let toggleRestMutation = ctx.useMutation props.OnToggleRest

                let workToggleLabel =
                    ctx.useDerived1 (props.Status, (fun s -> if s.IsActive then "勤務終了" else "勤務開始"))

                let restToggleLabel =
                    ctx.useDerived1 (props.Status, (fun s -> if s.IsResting then "休憩終了" else "休憩開始"))

                let workToggleEnabled =
                    ctx.useDerived1 (toggleWorkMutation.IsPending, (fun isPending -> not isPending))

                let restToggleEnabled =
                    ctx.useDerived2 ((toggleRestMutation.IsPending, status), (fun (r, s) -> not r && s.IsActive))

                Grid.create
                    [ Grid.rowDefinitions "Auto"
                      Grid.columnDefinitions "*,*"
                      Grid.children
                          [ ToggleButton.create
                                [ Grid.column 0
                                  Button.content (CjkTextBlock.create [ TextBlock.text workToggleLabel.Current ])
                                  Button.onClick (fun _ -> toggleWorkMutation.Mutate())
                                  Button.isEnabled workToggleEnabled.Current
                                  ToggleButton.isChecked status.Current.IsWorking
                                  Button.height 50.0
                                  Button.horizontalAlignment HorizontalAlignment.Stretch
                                  Button.horizontalContentAlignment HorizontalAlignment.Center
                                  Button.verticalContentAlignment VerticalAlignment.Center
                                  Button.fontSize 16.0
                                  Button.margin (Thickness(0, 0, 10.0, 0)) ]
                            ToggleButton.create
                                [ Grid.column 1
                                  Button.content (CjkTextBlock.create [ TextBlock.text restToggleLabel.Current ])
                                  Button.onClick (fun _ -> toggleRestMutation.Mutate())
                                  Button.isEnabled restToggleEnabled.Current
                                  ToggleButton.isChecked status.Current.IsResting
                                  Button.height 50.0
                                  Button.horizontalAlignment HorizontalAlignment.Stretch
                                  Button.horizontalContentAlignment HorizontalAlignment.Center
                                  Button.verticalContentAlignment VerticalAlignment.Center
                                  Button.fontSize 16.0 ] ] ]

        )
