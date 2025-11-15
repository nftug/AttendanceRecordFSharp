namespace AttendanceRecord.Presentation.Utils

open Material.Icons
open type NXUI.Builders
open NXUI.Extensions
open R3

module MaterialIcon =
    let create (kind: MaterialIconKind) : Avalonia.MaterialIcon =
        let icon = Avalonia.MaterialIcon()
        icon.Kind <- kind
        icon

module MaterialIconLabel =
    let create (kind: MaterialIconKind) (label: string) : Avalonia.Controls.Control =
        StackPanel()
            .OrientationHorizontal()
            .Spacing(10.0)
            .Children(MaterialIcon.create kind, TextBlock().Text(label))
