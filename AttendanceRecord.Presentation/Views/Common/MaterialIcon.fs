namespace AttendanceRecord.Presentation.Views.Common

open Material.Icons
open type NXUI.Builders
open NXUI.Extensions
open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared

module MaterialIcon =
    let create (kind: Observable<MaterialIconKind>) : Avalonia.MaterialIcon =
        let icon = Avalonia.MaterialIcon()

        let binding =
            Avalonia.MaterialIcon.KindProperty.Bind().WithMode(Avalonia.Data.BindingMode.OneWay)

        icon[binding] <- kind |> asBinding

        icon

type MaterialIconLabelProps =
    { Kind: Observable<MaterialIconKind>
      Label: Observable<string>
      Spacing: Observable<float option> }

module MaterialIconLabel =
    let create (props: MaterialIconLabelProps) =
        StackPanel()
            .OrientationHorizontal()
            .Spacing(props.Spacing |> R3.map (Option.defaultValue 10.0) |> asBinding)
            .Children(MaterialIcon.create props.Kind, TextBlock().Text(props.Label |> asBinding))

type MaterialIconButtonProps =
    { Kind: Observable<MaterialIconKind>
      OnClick: Avalonia.Interactivity.RoutedEventArgs -> unit
      FontSize: Observable<float option>
      Tooltip: Observable<string option> }

module MaterialIconButton =
    let create (props: MaterialIconButtonProps) : Avalonia.Controls.Button =
        Button()
            .Content(MaterialIcon.create props.Kind)
            .OnClickHandler(fun _ -> props.OnClick)
            .Background(Avalonia.Media.Brushes.Transparent)
            .BorderBrush(Avalonia.Media.Brushes.Transparent)
            .FontSize(props.FontSize |> R3.map (Option.defaultValue 14.0) |> asBinding)
            .Tip(props.Tooltip |> R3.map Option.toObj |> asBinding)
