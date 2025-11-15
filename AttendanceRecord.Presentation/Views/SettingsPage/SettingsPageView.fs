namespace AttendanceRecord.Presentation.Views.SettingsPage

open type NXUI.Builders
open NXUI.Extensions

module SettingsPageView =
    let create () : Avalonia.Controls.Control =
        StackPanel()
            .HorizontalAlignmentCenter()
            .VerticalAlignmentCenter()
            .Children(
                TextBlock()
                    .Text("Settings Page")
                    .FontSize(24.0)
                    .FontWeightBold()
                    .HorizontalAlignmentCenter()
            )
