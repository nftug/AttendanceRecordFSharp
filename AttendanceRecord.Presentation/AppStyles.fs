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
         [ Style(_.OfType<Button>())
              .SetContentControlHorizontalContentAlignment(HorizontalAlignment.Center)
              .SetContentControlVerticalContentAlignment(VerticalAlignment.Center)
           Style(_.OfType<ToggleButton>())
              .SetContentControlHorizontalContentAlignment(HorizontalAlignment.Center)
              .SetContentControlVerticalContentAlignment(VerticalAlignment.Center) ]
