namespace AttendanceRecord.Presentation

open NXUI.Extensions
open Avalonia.Styling
open Avalonia.Controls
open Avalonia.Layout

open Avalonia.Controls.Primitives

type AppStyles() =
    inherit Styles()

    do
        base.AddRange
            [ Style(fun x -> x.OfType<Button>())
                  .SetContentControlHorizontalContentAlignment(HorizontalAlignment.Center)
                  .SetContentControlVerticalContentAlignment(VerticalAlignment.Center)
              Style(fun x -> x.OfType<ToggleButton>())
                  .SetContentControlHorizontalContentAlignment(HorizontalAlignment.Center)
                  .SetContentControlVerticalContentAlignment(VerticalAlignment.Center) ]
