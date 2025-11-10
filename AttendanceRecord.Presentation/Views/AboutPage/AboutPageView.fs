namespace AttendanceRecord.Presentation.Views.AboutPage

open type NXUI.Builders
open NXUI.Extensions
open AttendanceRecord.Presentation.Utils
open System.Reflection

module AboutPageView =
    let create () : Avalonia.Controls.Control =
        let version = Assembly.GetEntryAssembly().GetName().Version.ToString()

        withReactive (fun _ _ ->
            StackPanel()
                .HorizontalAlignmentCenter()
                .VerticalAlignmentCenter()
                .Children(
                    TextBlock()
                        .Text("Attendance Record")
                        .FontSize(32.0)
                        .FontWeightBold()
                        .HorizontalAlignmentCenter()
                        .Margin(0.0, 0.0, 0.0, 30.0),
                    TextBlock()
                        .Text($"Version {version}")
                        .FontSize(24.0)
                        .HorizontalAlignmentCenter()
                ))
