namespace AttendanceRecord.Presentation.Views.Common

open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared

module ValidationErrorsText =
   let create (errors: R3.Observable<string list>) =
      ItemsControl()
         .ItemsSource(errors |> asBinding)
         .ItemTemplateFunc(fun (error: string) ->
            TextBlock()
               .Text(error)
               .Foreground(getDynamicBrushResource "SystemFillColorCriticalBrush" |> asBinding)
               .TextWrappingWrap()
               .FontSize(13.0))
         .IsVisible(errors |> R3.map (List.isEmpty >> not) |> asBinding)
