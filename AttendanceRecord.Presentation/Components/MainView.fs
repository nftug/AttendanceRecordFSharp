namespace AttendanceRecord.Presentation.Components

open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Controls
open AttendanceRecord.Presentation.Compositions

module MainView =
    let view (services: ServiceContainer) : Component =
        Component(fun _ ->
            StackPanel.create
                [ StackPanel.children [ TextBlock.create [ TextBlock.text "Attendance Record Main View" ] ] ])
