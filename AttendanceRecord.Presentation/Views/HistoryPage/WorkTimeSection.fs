namespace AttendanceRecord.Presentation.Views.HistoryPage

open System
open Avalonia.Media
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared

module WorkTimeSection =
    open NXUI.Extensions
    open type NXUI.Builders

    let create
        (editingRecord: R3.ReactiveProperty<WorkRecordDetailsDto option>)
        (isFormDirty: R3.ReactiveProperty<bool>)
        =
        withReactive (fun disposables _ ->
            let startedAt = R3.property (None: DateTime option) |> R3.disposeWith disposables
            let endedAt = R3.property (None: DateTime option) |> R3.disposeWith disposables

            editingRecord
            |> R3.subscribe (fun rOpt ->
                match rOpt with
                | Some r ->
                    startedAt.Value <- Some r.WorkTimeDuration.StartedAt
                    endedAt.Value <- r.WorkTimeDuration.EndedAt
                | None -> ())
            |> disposables.Add

            startedAt
            |> R3.combineLatest2 endedAt (fun sa ea -> sa, ea)
            |> R3.skip 1
            |> R3.subscribe (fun (s, e) ->
                match editingRecord.Value with
                | Some r ->
                    editingRecord.Value <-
                        Some
                            { r with
                                WorkTimeDuration.StartedAt =
                                    defaultArg s r.WorkTimeDuration.StartedAt
                                WorkTimeDuration.EndedAt = e }
                | _ -> ())
            |> disposables.Add

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(15.0)
                .Child(
                    StackPanel()
                        .Spacing(15.0)
                        .Children(
                            TextBlock().Text("出退勤").FontSize(18.0).FontWeightBold(),
                            StackPanel()
                                .OrientationHorizontal()
                                .Spacing(15.0)
                                .Children(
                                    TimePickerField.create
                                        { Label = "出勤時間"
                                          DateValue = defaultArg startedAt.Value DateTime.MinValue
                                          SelectedDateTime = startedAt
                                          IsDirty = isFormDirty },
                                    TimePickerField.create
                                        { Label = "退勤時間"
                                          DateValue = defaultArg endedAt.Value DateTime.MinValue
                                          SelectedDateTime = endedAt
                                          IsDirty = isFormDirty }
                                )
                        )
                ))
