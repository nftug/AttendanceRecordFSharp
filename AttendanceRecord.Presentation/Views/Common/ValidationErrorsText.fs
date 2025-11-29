namespace AttendanceRecord.Presentation.Views.Common

open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common.Context
open AttendanceRecord.Shared

type ValidationErrorsTextProps =
    { Errors: R3.Observable<string list>
      FontSize: float option }

module ValidationErrorsText =
    open NXUI.Extensions
    open type NXUI.Builders

    let create (props: ValidationErrorsTextProps) =
        withLifecycle (fun _ self ->
            let themeCtx = Context.require<ThemeContext> self |> fst
            let textColor = themeCtx.GetBrushResourceObservable "SystemFillColorCriticalBrush"

            ItemsControl()
                .ItemsSource(props.Errors |> asBinding)
                .ItemTemplateFunc(fun (error: string) ->
                    TextBlock()
                        .Text(error)
                        .Foreground(textColor |> asBinding)
                        .FontSize(props.FontSize |> Option.defaultValue 14.0)
                        .Margin(0.0, 2.0)))
        |> _.IsVisible(props.Errors |> R3.map (List.isEmpty >> not) |> asBinding)
