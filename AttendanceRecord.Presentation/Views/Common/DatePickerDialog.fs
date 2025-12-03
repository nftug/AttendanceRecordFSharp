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
    open FluentAvalonia.UI.Controls

    let create
        (props: DatePickerDialogProps)
        (selectedDate: R3.ReactiveProperty<DateTime option>)
        (disposables: R3.CompositeDisposable)
        =
        let content =
            let onDisplayDateChanged (newDate: DateTime) =
                match selectedDate.Value with
                | Some date when date.Month <> newDate.Month -> selectedDate.Value <- None
                | _ -> ()

            Calendar()
                .Margin(15.0, 0.0)
                .DisplayDate(props.InitialMonth |> Option.defaultValue DateTime.Now.Date)
                .OnDisplayDateChanged(fun ctl e ->
                    e.Subscribe(fun _ -> onDisplayDateChanged ctl.DisplayDate) |> disposables.Add)
                .SelectedDate(selectedDate |> R3.map Option.toNullable |> asBinding)
                .OnSelectedDateChanged(fun ctl e ->
                    e.Subscribe(fun _ ->
                        selectedDate.Value <- ctl.SelectedDate |> Option.ofNullable)
                    |> disposables.Add)
                .HorizontalAlignmentCenter()

        let dialog = ContentDialog()
        dialog.Title <- "日付を選択"
        dialog.Content <- content
        dialog.PrimaryButtonText <- "選択"
        dialog.CloseButtonText <- "キャンセル"

        selectedDate
        |> R3.map Option.isSome
        |> R3.subscribe (fun hasDate -> dialog.IsPrimaryButtonEnabled <- hasDate)
        |> disposables.Add

        dialog

module DatePickerDialog =
    open System.Threading
    open System.Threading.Tasks
    open FluentAvalonia.UI.Controls

    let show (props: DatePickerDialogProps) (ct: CancellationToken option) : Task<DateTime option> =
        task {
            let disposables = new R3.CompositeDisposable()
            let selectedDate = R3.property props.InitialDate |> R3.disposeWith disposables

            let dialog = DatePickerDialogView.create props selectedDate disposables
            let! dialogResult = dialog.ShowAsync()

            ct |> Option.iter _.ThrowIfCancellationRequested()

            try
                return
                    if dialogResult = ContentDialogResult.Primary then
                        selectedDate.CurrentValue
                    else
                        None
            finally
                disposables.Dispose()
        }
