namespace AttendanceRecord.Presentation.Views.SettingsPage

open type NXUI.Builders
open NXUI.Extensions
open AttendanceRecord.Presentation.Utils

module SettingsPageView =
    let view () : Avalonia.Controls.Control =
        withReactive (fun _ _ ->
            StackPanel()
                .HorizontalAlignmentCenter()
                .VerticalAlignmentCenter()
                .Children(
                    TextBlock()
                        .Text("Settings Page")
                        .FontSize(24.0)
                        .FontWeightBold()
                        .HorizontalAlignmentCenter()
                ))
