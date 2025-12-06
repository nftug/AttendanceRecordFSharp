namespace AttendanceRecord.Presentation.Views.Common

open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils

module ControlBorder =
   let create () =
      Border()
         .BorderThickness(1.0)
         .BorderBrush(getDynamicBrushResource "ControlElevationBorderBrush" |> asBinding)
         .Background(getDynamicBrushResource "CardBackgroundFillColorDefaultBrush" |> asBinding)
         .CornerRadius(4.0)
