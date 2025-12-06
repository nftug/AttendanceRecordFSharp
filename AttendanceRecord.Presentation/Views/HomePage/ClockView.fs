namespace AttendanceRecord.Presentation.Views.HomePage

open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open R3
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Views.HomePage.Context

module ClockView =
   let create () : Avalonia.Controls.Control =
      withLifecycle (fun _ self ->
         let ctx, _ = Context.require<HomePageContext> self
         let now = ctx.Status |> R3.map _.CurrentTime

         TextBlock()
            .Text(now |> R3.map _.ToString("HH:mm:ss") |> asBinding)
            .FontSize(68.0)
            .Margin(20.0)
            .HorizontalAlignmentCenter()
            .VerticalAlignmentCenter())
