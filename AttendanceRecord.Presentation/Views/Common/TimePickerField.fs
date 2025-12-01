namespace AttendanceRecord.Presentation.Views.Common

open System
open R3
open NXUI.Extensions
open type NXUI.Builders
open FluentAvalonia.UI.Controls
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared

type TimePickerFieldProps =
    { Label: Observable<string>
      BaseDate: Observable<DateTime> option
      Value: Observable<DateTime option>
      OnSetValue: (DateTime option -> unit)
      IsClearable: Observable<bool>
      HasErrors: Observable<bool> }

module TimePickerField =
    let create (props: TimePickerFieldProps) =
        withLifecycle (fun disposables _ ->
            let baseDate =
                props.BaseDate
                |> Option.defaultWith (fun () -> R3.ret DateTime.Today)
                |> R3.map _.Date
                |> R3.readonly None
                |> R3.disposeWith disposables

            let handleTimeChange (timeOpt: TimeSpan option) =
                props.OnSetValue(timeOpt |> Option.map ((+) baseDate.CurrentValue))

            let isClearable =
                R3.combineLatest2 props.IsClearable props.Value
                |> R3.map (fun (isClearable, value) -> isClearable && Option.isSome value)

            StackPanel()
                .Spacing(5.0)
                .Children(
                    TextBlock().Text(props.Label |> asBinding).FontSize(12.0),
                    StackPanel()
                        .OrientationHorizontal()
                        .Spacing(3.0)
                        .Children(
                            TimePicker()
                                .SelectedTime(
                                    props.Value
                                    |> R3.map (Option.map _.TimeOfDay)
                                    |> R3.map Option.toNullable
                                    |> asBinding
                                )
                                .OnSelectedTimeChanged(fun ctl e ->
                                    e.Subscribe(fun _ ->
                                        handleTimeChange (Option.ofNullable ctl.SelectedTime))
                                    |> disposables.Add)
                                .HasErrors(props.HasErrors |> asBinding),
                            SymbolIconButton.create
                                { Symbol = Symbol.Clear |> R3.ret
                                  OnClick = fun _ -> handleTimeChange None
                                  FontSize = Some 12.0 |> R3.ret
                                  Tooltip = Some "時刻をクリア" |> R3.ret }
                            |> _.IsEnabled(isClearable |> asBinding)
                            |> _.Opacity(
                                isClearable
                                |> R3.map (fun isClearable -> if isClearable then 1.0 else 0.0)
                                |> asBinding
                            )
                        )
                ))
