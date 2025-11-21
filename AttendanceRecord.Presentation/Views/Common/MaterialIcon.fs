namespace AttendanceRecord.Presentation.Views.Common

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

type MaterialIconButtonProps =
    { Kind: MaterialIconKind
      OnClick: Avalonia.Interactivity.RoutedEventArgs -> unit
      FontSize: float option
      Tooltip: string option }

module MaterialIconButton =
    let create (props: MaterialIconButtonProps) : Avalonia.Controls.Control =
        let button =
            Button()
                .Content(MaterialIcon.create props.Kind)
                .OnClickHandler(fun _ e -> props.OnClick e)
                .Background(Avalonia.Media.Brushes.Transparent)
                .BorderBrush(Avalonia.Media.Brushes.Transparent)

        match props.FontSize with
        | Some size -> button.FontSize size
        | None -> button
        |> fun b ->
            match props.Tooltip with
            | Some tip -> b.Tip tip
            | None -> b
