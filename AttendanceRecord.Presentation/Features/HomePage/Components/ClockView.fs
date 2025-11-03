namespace AttendanceRecord.Presentation.Features.HomePage.Components

open Avalonia
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open Avalonia.Layout

open AttendanceRecord.Application.Dtos.Responses

type ClockViewProps =
    { Status: IReadable<CurrentStatusDto> }

module ClockView =
    let mainWindow = ApplicationUtils.tryGetMainWindow ()

    let view (props: ClockViewProps) =
        Component.create (
            "ClockView",
            fun ctx ->
                let now = ctx.useDerived1 (props.Status, _.CurrentTime)

                ctx.useEffect (
                    fun () ->
                        match mainWindow with
                        | Some window ->
                            window.Title <- $"""Attendance Record - {now.Current.ToString "HH:mm:ss"}"""
                        | None -> ()
                    , [ EffectTrigger.AfterChange now ]
                )

                CjkTextBlock.create
                    [ TextBlock.text (now.Current.ToString "HH:mm:ss")
                      TextBlock.fontSize 68.0
                      TextBlock.margin 20.0
                      TextBlock.horizontalAlignment HorizontalAlignment.Center
                      TextBlock.verticalAlignment VerticalAlignment.Center ]
        )
