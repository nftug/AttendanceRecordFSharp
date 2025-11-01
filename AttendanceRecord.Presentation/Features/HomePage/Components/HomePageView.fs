namespace AttendanceRecord.Presentation.Features.HomePage.Components

open R3
open FsToolkit.ErrorHandling
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Presentation.Features.HomePage.Hooks

type HomePageViewProps =
    { StatusObservable: Observable<CurrentStatusDto>
      OnToggleWork: unit -> TaskResult<CurrentStatusDto, string>
      OnToggleRest: unit -> TaskResult<CurrentStatusDto, string> }

module HomePageView =
    let view (props: HomePageViewProps) =
        Component.create (
            "HomePageView",
            fun ctx ->
                let hooks = useHomePageHooks props.StatusObservable ctx
                let status = hooks.Status

                if status.Current.IsNone then
                    TextBlock.create
                        [ TextBlock.text "Loading..."
                          TextBlock.fontSize 24.0
                          TextBlock.horizontalAlignment HorizontalAlignment.Center
                          TextBlock.verticalAlignment VerticalAlignment.Center ]
                else
                    let status = ctx.useDerived1 (status, Option.get)

                    StackPanel.create
                        [ StackPanel.margin 20.0
                          StackPanel.spacing 10.0
                          StackPanel.horizontalAlignment HorizontalAlignment.Center
                          StackPanel.verticalAlignment VerticalAlignment.Center
                          StackPanel.children
                              [ ClockView.view { Status = status }
                                HomeActionsView.view
                                    { Status = status
                                      OnToggleWork = props.OnToggleWork
                                      OnToggleRest = props.OnToggleRest } ] ]
        )
