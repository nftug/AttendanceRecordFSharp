namespace AttendanceRecord.Presentation.Utils

open System.Threading.Tasks
open System
open Avalonia.Controls

module TransparentWindow =
    let useWindow (action: Window -> Task<'T>) : Task<'T> =
        task {
            // Show transparent window to host a dialog window
            let transparentWindow =
                Window(
                    Title = getApplicationTitle (),
                    CanResize = false,
                    ShowInTaskbar = false,
                    Opacity = 0.0,
                    Width = 1.0,
                    Height = 1.0,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    SystemDecorations = SystemDecorations.None,
                    Focusable = false
                )

            if OperatingSystem.IsMacOS() then
                transparentWindow.Width <- 0.0
                transparentWindow.Height <- 0.0

            transparentWindow.Show()
            transparentWindow.Activate()

            let! result = action transparentWindow
            transparentWindow.Close()

            return result
        }
