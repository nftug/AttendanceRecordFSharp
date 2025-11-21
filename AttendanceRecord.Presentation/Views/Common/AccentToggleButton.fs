namespace AttendanceRecord.Presentation.Views.Common

open NXUI.Extensions
open Avalonia.Controls.Primitives
open R3
open AttendanceRecord.Presentation.Utils

module AccentToggleButton =
    let create (isChecked: ReadOnlyReactiveProperty<bool>) : ToggleButton =
        ToggleButton()
            .IsChecked(isChecked |> asBinding)
            .OnIsCheckedChangedHandler(fun ctl _ -> ctl.IsChecked <- isChecked.CurrentValue)
