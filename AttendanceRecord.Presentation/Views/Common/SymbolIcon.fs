namespace AttendanceRecord.Presentation.Views.Common

open FluentAvalonia.UI.Controls
open type NXUI.Builders
open NXUI.Extensions
open R3
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared
open System.Runtime.CompilerServices

[<Extension>]
type __SymbolIconExtensions =
    [<Extension>]
    static member Foreground(icon: SymbolIcon, brush: Avalonia.Media.IBrush) : SymbolIcon =
        icon.Foreground <- brush
        icon

module SymbolIcon =
    let create (symbol: Observable<Symbol>) : SymbolIcon =
        let icon = SymbolIcon()

        let binding =
            SymbolIcon.SymbolProperty.Bind().WithMode(Avalonia.Data.BindingMode.OneWay)

        icon[binding] <- symbol |> asBinding

        icon

type SymbolIconLabelProps =
    { Symbol: Observable<Symbol>
      Label: Observable<string>
      Spacing: Observable<float option> }

module SymbolIconLabel =
    let create (props: SymbolIconLabelProps) =
        StackPanel()
            .OrientationHorizontal()
            .Spacing(props.Spacing |> R3.map (Option.defaultValue 10.0) |> asBinding)
            .Children(
                SymbolIcon.create props.Symbol,
                TextBlock().Text(props.Label |> asBinding).VerticalAlignmentCenter()
            )

type SymbolIconButtonProps =
    { Symbol: Observable<Symbol>
      OnClick: Avalonia.Interactivity.RoutedEventArgs -> unit
      FontSize: Observable<float option>
      Tooltip: Observable<string option> }

module SymbolIconButton =
    let create (props: SymbolIconButtonProps) : Avalonia.Controls.Button =
        Button()
            .Content(SymbolIcon.create props.Symbol)
            .OnClickHandler(fun _ -> props.OnClick)
            .Background(Avalonia.Media.Brushes.Transparent)
            .BorderBrush(Avalonia.Media.Brushes.Transparent)
            .FontSize(props.FontSize |> R3.map (Option.defaultValue 14.0) |> asBinding)
            .Tip(props.Tooltip |> R3.map Option.toObj |> asBinding)
