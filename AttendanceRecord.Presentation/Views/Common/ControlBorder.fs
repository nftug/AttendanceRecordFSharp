namespace AttendanceRecord.Presentation.Views.Common

open AttendanceRecord.Presentation.Views.Common.Context
open AttendanceRecord.Presentation.Utils

module ControlBorder =
    open NXUI.Extensions
    open type NXUI.Builders

    let create (themeCtx: ThemeContext) =
        let borderBrush = themeCtx.GetBrushResourceObservable "ControlElevationBorderBrush"

        let backgroundBrush =
            themeCtx.GetBrushResourceObservable "CardBackgroundFillColorDefaultBrush"

        Border()
            .BorderThickness(1.0)
            .BorderBrush(borderBrush |> asBinding)
            .Background(backgroundBrush |> asBinding)
            .CornerRadius(4.0)
