namespace AttendanceRecord.Presentation.Features.HomePage.Components

open FsToolkit.ErrorHandling
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open Avalonia

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
                    [ Grid.width 300.0
                      Grid.rowDefinitions "Auto"
                      Grid.columnDefinitions "*,*"
                      Grid.children
                          [ Button.create
                                [ Grid.column 0
                                  Button.content workToggleLabel.Current
                                  Button.onClick (fun _ -> toggleWorkMutation.Mutate())
                                  Button.isEnabled workToggleEnabled.Current
                                  Button.horizontalAlignment HorizontalAlignment.Stretch
                                  Button.horizontalContentAlignment HorizontalAlignment.Center
                                  Button.padding (Thickness(0, 8.0, 0, 0))
                                  Button.margin (Thickness(5.0, 0.0)) ]
                            Button.create
                                [ Grid.column 1
                                  Button.content restToggleLabel.Current
                                  Button.onClick (fun _ -> toggleRestMutation.Mutate())
                                  Button.isEnabled restToggleEnabled.Current
                                  Button.horizontalAlignment HorizontalAlignment.Stretch
                                  Button.horizontalContentAlignment HorizontalAlignment.Center
                                  Button.padding (Thickness(0, 8.0, 0, 0))
                                  Button.margin (Thickness(5.0, 0.0)) ] ] ]

        )
