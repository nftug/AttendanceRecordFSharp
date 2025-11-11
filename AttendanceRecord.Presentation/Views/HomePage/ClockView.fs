namespace AttendanceRecord.Presentation.Views.HomePage

open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open R3
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Shared

module ClockView =
    let create () : Avalonia.Controls.Control =
        withReactive (fun _ self ->
            let ctx, _ = HomePageContextProvider.require self
            let now = ctx.Status |> R3.map _.CurrentTime

            TextBlock()
                .Text(now |> R3.map (fun v -> v.ToString "HH:mm:ss") |> asBinding)
                .FontSize(68.0)
                .Margin(20.0)
                .HorizontalAlignmentCenter()
                .VerticalAlignmentCenter())
