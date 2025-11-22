namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open Avalonia.Media
open NXUI.Extensions
open type NXUI.Builders

module ThemeSection =
    let create () =
        let createRadioButton (text: string) (groupName: string) (isChecked: bool) =
            RadioButton()
                .Content(text)
                .GroupName(groupName)
                .IsChecked(isChecked)
                .Margin(0.0, 0.0, 20.0, 0.0)

        Border()
            .BorderThickness(1.0)
            .BorderBrush(Brushes.Gray)
            .Padding(20.0)
            .Child(
                StackPanel()
                    .Spacing(15.0)
                    .Children(
                        TextBlock().Text("テーマ設定").FontSize(18.0).FontWeightBold(),
                        StackPanel()
                            .OrientationHorizontal()
                            .Spacing(10.0)
                            .Children(
                                createRadioButton "システム" "ThemeGroup" true,
                                createRadioButton "ライト" "ThemeGroup" false,
                                createRadioButton "ダーク" "ThemeGroup" false
                            )
                    )
            )
