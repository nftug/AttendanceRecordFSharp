namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open R3
open System
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared

type TimeDurationPickerProps =
    { StartedAt: Observable<DateTime>
      EndedAt: Observable<DateTime option>
      OnStartedAtChanged: DateTime option -> unit
      OnEndedAtChanged: DateTime option -> unit
      Errors: Observable<string list>
      Spacing: float option
      IsEndedAtClearable: Observable<bool> }

module TimeDurationPicker =
    open NXUI.Extensions
    open type NXUI.Builders

    let create (props: TimeDurationPickerProps) =
        withLifecycle (fun _ self ->
            let ctx = Context.require<HistoryPageContext> self |> fst

            StackPanel()
                .Spacing(3.0)
                .Children(
                    StackPanel()
                        .OrientationHorizontal()
                        .Spacing(props.Spacing |> Option.defaultValue 15.0)
                        .Children(
                            TimePickerField.create
                                { Label = "開始時間" |> R3.ret
                                  BaseDate = Some ctx.CurrentDate
                                  Value = props.StartedAt |> R3.map Some
                                  OnSetValue = props.OnStartedAtChanged
                                  IsClearable = false |> R3.ret
                                  HasErrors = props.Errors |> R3.map (not << List.isEmpty) },
                            TimePickerField.create
                                { Label = "終了時間" |> R3.ret
                                  BaseDate = Some ctx.CurrentDate
                                  Value = props.EndedAt
                                  OnSetValue = props.OnEndedAtChanged
                                  IsClearable = props.IsEndedAtClearable
                                  HasErrors = props.Errors |> R3.map (not << List.isEmpty) }
                        ),
                    ValidationErrorsText.create
                        { Errors = props.Errors
                          FontSize = None }
                ))
