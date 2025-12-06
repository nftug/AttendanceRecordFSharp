namespace AttendanceRecord.Presentation.Views.Common

open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared

type ValidationErrorsTextProps =
   { Errors: R3.Observable<string list>
     FontSize: float option }

module ValidationErrorsText =
   let create (props: ValidationErrorsTextProps) =
      ItemsControl()
         .ItemsSource(props.Errors |> asBinding)
         .ItemTemplateFunc(fun (error: string) ->
            TextBlock()
               .Text(error)
               .Foreground(getDynamicBrushResource "SystemFillColorCriticalBrush" |> asBinding)
               .TextWrappingWrap()
               .FontSize(props.FontSize |> Option.defaultValue 13.0))
         .IsVisible(props.Errors |> R3.map (List.isEmpty >> not) |> asBinding)
