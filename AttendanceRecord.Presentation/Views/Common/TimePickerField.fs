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
      SelectedDateTime: ReactiveProperty<DateTime option>
      IsClearable: bool }

module TimePickerField =
    let create (props: TimePickerFieldProps) =
        props.BaseDate
        |> toViewWithReactive (fun baseDateOpt disposables _ ->
            match baseDateOpt with
            | None -> Panel()
            | Some baseDate ->
                let handleTimeChange (timeOpt: TimeSpan option) =
                    props.SelectedDateTime.Value <-
                        match timeOpt with
                        | Some time -> Some(baseDate + time)
                        | _ -> None

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
                                        props.SelectedDateTime
                                        |> R3.map (fun dt ->
                                            dt |> Option.map (fun d -> d.TimeOfDay))
                                        |> R3.map (fun tOpt -> tOpt |> Option.toNullable)
                                        |> asBinding
                                    )
                                    .OnSelectedTimeChanged(fun ctl e ->
                                        e.Subscribe(fun _ ->
                                            handleTimeChange (
                                                ctl.SelectedTime |> Option.ofNullable
                                            ))
                                        |> disposables.Add),
                                (MaterialIconButton.create
                                    { Kind = MaterialIconKind.Close
                                      OnClick = fun _ -> handleTimeChange None
                                      FontSize = Some 12.0
                                      Tooltip = Some "時間をクリア" })
                                    .IsVisible(props.IsClearable)
                            )
                    ))
