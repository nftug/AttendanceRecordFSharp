namespace AttendanceRecord.Presentation.Utils

open Avalonia.Media

type AccentColors =
    { Background: IBrush
      Foreground: IBrush }

module PlatformColors =
    let getAccentColors () : AccentColors =
        let background = getPlatformColors().AccentColor1

        // 相対輝度を計算して適切な前景色を選択
        let foreground =
            let r = float background.R / 255.0
            let g = float background.G / 255.0
            let b = float background.B / 255.0
            let luminance = 0.299 * r + 0.587 * g + 0.114 * b

            if luminance > 0.5 then
                SolidColorBrush(Colors.Black) :> IBrush
            else
                SolidColorBrush(Colors.White) :> IBrush

        { Background = background |> SolidColorBrush :> IBrush
          Foreground = foreground }
