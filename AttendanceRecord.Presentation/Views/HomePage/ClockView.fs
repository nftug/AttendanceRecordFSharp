namespace AttendanceRecord.Presentation.Views.HomePage

open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open R3
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Shared

type ClockViewProps =
    { Status: Observable<CurrentStatusDto> }

module ClockView =
    let create (props: ClockViewProps) : Avalonia.Controls.Control =
        withReactive (fun _ _ ->
            let now = props.Status |> R3.map _.CurrentTime

            TextBlock()
                .Text(now |> R3.map (fun v -> v.ToString "HH:mm:ss") |> asBinding)
                .FontSize(68.0)
                .Margin(20.0)
                .HorizontalAlignmentCenter()
                .VerticalAlignmentCenter())
