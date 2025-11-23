namespace AttendanceRecord.Presentation.Views.Common

open System
open R3
open Material.Icons
open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared

type TimePickerFieldProps =
    { Label: Observable<string>
      BaseDate: Observable<DateTime option>
      Value: Observable<DateTime option>
      OnSetValue: (DateTime option -> unit)
      IsClearable: Observable<bool> }

module TimePickerField =
    let create (props: TimePickerFieldProps) =
        withLifecycle (fun disposables _ ->
            let baseDate =
                props.BaseDate
                |> R3.map (fun dateOpt -> defaultArg dateOpt DateTime.Today |> _.Date)
                |> R3.readonly None
                |> R3.disposeWith disposables

            let handleTimeChange (timeOpt: TimeSpan option) =
                props.OnSetValue(
                    match timeOpt with
                    | Some time -> Some(baseDate.CurrentValue + time)
                    | _ -> None
                )

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
                                    |> disposables.Add),
                            MaterialIconButton.create
                                { Kind = MaterialIconKind.Close |> R3.ret
                                  OnClick = fun _ -> handleTimeChange None
                                  FontSize = Some 12.0 |> R3.ret
                                  Tooltip = Some "時間をクリア" |> R3.ret }
                            |> _.IsEnabled(props.Value |> R3.map Option.isSome |> asBinding)
                                .IsVisible(props.IsClearable |> asBinding)
                        )
                ))
