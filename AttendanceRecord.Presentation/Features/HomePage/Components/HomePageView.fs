namespace AttendanceRecord.Presentation.Features.HomePage.Components

open R3
open FsToolkit.ErrorHandling
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout
open AttendanceRecord.Application.Dtos.Responses

type HomePageViewProps =
    { StatusObservable: Observable<CurrentStatusDto>
      OnToggleWork: unit -> TaskResult<CurrentStatusDto, string>
      OnToggleRest: unit -> TaskResult<CurrentStatusDto, string> }

module HomePageView =
    let view (props: HomePageViewProps) =
        Component.create (
            "HomePageView",
            fun ctx ->
                let status = ctx.useObservableState props.StatusObservable

                if status.Current.IsNone then
                    TextBlock.create
                        [ TextBlock.text "Loading..."
                          TextBlock.fontSize 24.0
                          TextBlock.horizontalAlignment HorizontalAlignment.Center
                          TextBlock.verticalAlignment VerticalAlignment.Center ]
                else
                    let status = ctx.useDerived1 (status, Option.get)

                    DockPanel.create
                        [ DockPanel.lastChildFill true
                          DockPanel.children
                              [ StackPanel.create
                                    [ DockPanel.dock Dock.Bottom
                                      StackPanel.margin 20.0
                                      StackPanel.spacing 25.0
                                      StackPanel.horizontalAlignment HorizontalAlignment.Stretch
                                      StackPanel.verticalAlignment VerticalAlignment.Center
                                      StackPanel.children
                                          [ HomeActionsView.view
                                                { Status = status
                                                  OnToggleWork = props.OnToggleWork
                                                  OnToggleRest = props.OnToggleRest }
                                            StatusView.view { Status = status } ] ]
                                ClockView.view { Status = status } ] ]
        )
