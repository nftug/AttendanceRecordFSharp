namespace AttendanceRecord.Presentation

open Avalonia.Styling
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Layout

type AppStyles() as this =
    inherit Styles()

    do
        let buttonStyle = Style(fun x -> x.OfType<Button>())
        buttonStyle.Add(Setter(Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Center))
        buttonStyle.Add(Setter(Button.VerticalContentAlignmentProperty, VerticalAlignment.Center))
        this.Add buttonStyle

        let toggleButtonStyle = Style(fun x -> x.OfType<ToggleButton>())
        toggleButtonStyle.Add(Setter(ToggleButton.HorizontalContentAlignmentProperty, HorizontalAlignment.Center))
        toggleButtonStyle.Add(Setter(ToggleButton.VerticalContentAlignmentProperty, VerticalAlignment.Center))
        this.Add toggleButtonStyle
