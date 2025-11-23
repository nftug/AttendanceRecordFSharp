namespace AttendanceRecord.Presentation.Views.Common

open Avalonia.Controls.Primitives
open R3
open AttendanceRecord.Presentation.Utils

[<AutoOpen>]
module private AccentToggleButtonHelpers =
    let applyAccentStyle (isChecked: Observable<bool>) (ctl: ToggleButton) =
        task {
            let! isChecked = isChecked.FirstAsync()
            ctl.IsChecked <- isChecked
        }
        |> ignore

module AccentToggleButton =
    open NXUI.Extensions

    let create (isChecked: Observable<bool>) : ToggleButton =
        ToggleButton()
            .IsChecked(isChecked |> asBinding)
            .OnIsCheckedChangedHandler(fun ctl _ -> applyAccentStyle isChecked ctl)
