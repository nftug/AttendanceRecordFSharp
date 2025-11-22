namespace AttendanceRecord.Presentation.Utils

open Avalonia.Media
open Avalonia.Controls.Primitives

module Colors =
    let determineForeground (background: IBrush) : IBrush =
        match background with
        | :? SolidColorBrush as solidBrush ->
            let r = float solidBrush.Color.R / 255.0
            let g = float solidBrush.Color.G / 255.0
            let b = float solidBrush.Color.B / 255.0
            let luminance = 0.299 * r + 0.587 * g + 0.114 * b

            if luminance > 0.5 then
                SolidColorBrush(Colors.Black) :> IBrush
            else
                SolidColorBrush(Colors.White) :> IBrush
        | _ -> SolidColorBrush(Colors.Black) :> IBrush

    let setAccentColorBackground (control: 'a :> TemplatedControl) : 'a =
        let background = getPlatformColors().AccentColor1 |> SolidColorBrush :> IBrush
        let foreground = determineForeground background

        control.Background <- background
        control.Foreground <- foreground

        control
