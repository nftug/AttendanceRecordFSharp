namespace AttendanceRecord.Presentation.Views.Common

open System
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared

type DatePickerDialogProps =
    { InitialDate: DateTime option
      InitialMonth: DateTime option }

module private DatePickerDialogView =
    open NXUI.Extensions
    open type NXUI.Builders
    open System.Windows.Input

    let create (props: DatePickerDialogProps) (closeCommand: ICommand) =
        withReactive (fun disposables _ ->
            let selectedDate = R3.property props.InitialDate |> R3.disposeWith disposables

            let canSelect = selectedDate |> R3.map (fun d -> d.IsSome)

            let onDisplayDateChanged (newDate: DateTime) =
                match selectedDate.Value with
                | Some date when date.Month <> newDate.Month -> selectedDate.Value <- None
                | _ -> ()

            StackPanel()
                .Margin(20.0)
                .Spacing(20.0)
                .MinWidth(400.0)
                .MaxWidth(500.0)
                .Children(
                    TextBlock().Text("日付を選択").FontSize(20.0).FontWeightBold().Margin(0, 0, 0, 10.0),
                    Calendar()
                        .DisplayDate(props.InitialMonth |> Option.defaultValue DateTime.Now.Date)
                        .OnDisplayDateChanged(fun ctl e ->
                            e.Subscribe(fun _ -> onDisplayDateChanged ctl.DisplayDate)
                            |> disposables.Add)
                        .SelectedDate(
                            selectedDate |> R3.map (fun d -> d |> Option.toNullable) |> asBinding
                        )
                        .OnSelectedDateChanged(fun ctl e ->
                            e.Subscribe(fun _ ->
                                selectedDate.Value <- ctl.SelectedDate |> Option.ofNullable)
                            |> disposables.Add)
                        .HorizontalAlignmentCenter(),
                    StackPanel()
                        .OrientationHorizontal()
                        .HorizontalAlignmentRight()
                        .Margin(0, 10.0, 0, 0)
                        .Spacing(10.0)
                        .Children(
                            Button()
                                .Content("キャンセル")
                                .OnClickHandler(fun _ _ -> closeCommand.Execute None)
                                .MinWidth(80.0)
                                .Height(35.0),
                            Button()
                                .Content("選択")
                                .OnClickHandler(fun _ _ -> closeCommand.Execute selectedDate.Value)
                                .IsEnabled(canSelect |> asBinding)
                                .MinWidth(80.0)
                                .Height(35.0)
                        )
                ))

module DatePickerDialog =
    open DialogHostAvalonia
    open System.Threading
    open System.Threading.Tasks

    let show (props: DatePickerDialogProps) (ct: CancellationToken option) : Task<DateTime option> =
        task {
            let dialogHost = getControlFromMainWindow<DialogHost> ()

            let! result =
                DatePickerDialogView.create props dialogHost.CloseDialogCommand
                |> DialogHost.Show

            match ct with
            | Some cancellationToken -> cancellationToken.ThrowIfCancellationRequested()
            | None -> ()

            return result :?> DateTime option
        }
