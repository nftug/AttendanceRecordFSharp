namespace AttendanceRecord.Presentation.Views.Common

open System
open R3
open Material.Icons
open NXUI.Extensions
open type NXUI.Builders
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared

type TimePickerFieldProps =
    { Label: string
      BaseDate: Observable<DateTime option>
      Value: Observable<DateTime option>
      OnSetValue: (DateTime option -> unit)
      IsClearable: bool }

module TimePickerField =
    let create (props: TimePickerFieldProps) =
        props.BaseDate
        |> toViewWithReactive (fun baseDateOpt disposables _ ->
            match baseDateOpt with
            | None -> Panel()
            | Some baseDate ->
                let handleTimeChange (timeOpt: TimeSpan option) =
                    props.OnSetValue(
                        match timeOpt with
                        | Some time -> Some(baseDate + time)
                        | _ -> None
                    )

                StackPanel()
                    .Spacing(5.0)
                    .Children(
                        TextBlock().Text(props.Label).FontSize(12.0),
                        StackPanel()
                            .OrientationHorizontal()
                            .Spacing(2.0)
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
                                (MaterialIconButton.create
                                    { Kind = MaterialIconKind.Close
                                      OnClick = fun _ -> handleTimeChange None
                                      FontSize = Some 12.0
                                      Tooltip = Some "時間をクリア" })
                                    .IsVisible(props.IsClearable)
                            )
                    ))
