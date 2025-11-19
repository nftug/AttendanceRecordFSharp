namespace AttendanceRecord.Presentation.Views.HistoryPage

open System
open Avalonia.Media
open Material.Icons
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Shared

module RestTimeSection =
    open NXUI.Extensions
    open type NXUI.Builders

    let private createRestTimesContent
        (editingRecord: R3.ReactiveProperty<WorkRecordDetailsDto option>)
        (isFormDirty: R3.ReactiveProperty<bool>)
        (disposables: R3.CompositeDisposable)
        =
        editingRecord
        |> toView (fun recordOpt ->
            match recordOpt with
            | None -> Panel()
            | Some record when record.RestTimes.IsEmpty ->
                TextBlock().Text("休憩記録がありません。").FontSize(14.0).Foreground(Brushes.Gray)
            | Some record ->
                let restItems =
                    record.RestTimes
                    |> List.map (fun rest ->
                        let restObservable =
                            editingRecord
                            |> R3.map (fun rOpt ->
                                rOpt
                                |> Option.bind (fun r ->
                                    r.RestTimes |> List.tryFind (fun rt -> rt.Id = rest.Id)))
                            |> R3.readonly None
                            |> R3.disposeWith disposables

                        let startedAt =
                            R3.property (Some rest.Duration.StartedAt)
                            |> R3.disposeWith disposables

                        let endedAt =
                            R3.property (rest.Duration.EndedAt) |> R3.disposeWith disposables

                        editingRecord
                        |> R3.subscribe (fun rOpt ->
                            match restObservable.CurrentValue with
                            | Some rt ->
                                startedAt.Value <- Some rt.Duration.StartedAt
                                endedAt.Value <- rt.Duration.EndedAt
                            | None -> ())
                        |> disposables.Add

                        startedAt
                        |> R3.combineLatest2 endedAt (fun sa ea -> sa, ea)
                        |> R3.skip 1
                        |> R3.subscribe (fun (s, e) ->
                            match editingRecord.Value with
                            | Some r ->
                                let updatedRestTimes =
                                    r.RestTimes
                                    |> List.map (fun rt ->
                                        if rt.Id = rest.Id then
                                            let s = defaultArg s rt.Duration.StartedAt

                                            { rt with
                                                Duration.StartedAt = s
                                                Duration.EndedAt = e }
                                        else
                                            rt)

                                editingRecord.Value <-
                                    Some { r with RestTimes = updatedRestTimes }

                                isFormDirty.Value <- true
                            | _ -> ())
                        |> disposables.Add

                        let onDelete () =
                            match editingRecord.Value with
                            | Some r ->
                                let updated =
                                    { r with
                                        RestTimes =
                                            r.RestTimes
                                            |> List.filter (fun rt -> rt.Id <> rest.Id) }

                                editingRecord.Value <- Some updated
                                isFormDirty.Value <- true
                            | None -> ()

                        StackPanel()
                            .OrientationHorizontal()
                            .Spacing(10.0)
                            .Margin(0, 5.0, 0, 5.0)
                            .Children(
                                TimePickerField.create
                                    { Label = "開始時間"
                                      DateValue = defaultArg startedAt.Value DateTime.MinValue
                                      SelectedDateTime = startedAt
                                      IsDirty = isFormDirty },
                                TimePickerField.create
                                    { Label = "終了時間"
                                      DateValue = defaultArg endedAt.Value DateTime.MinValue
                                      SelectedDateTime = endedAt
                                      IsDirty = isFormDirty },
                                Button()
                                    .Content(MaterialIcon.create MaterialIconKind.Delete)
                                    .OnClickHandler(fun _ _ -> onDelete ())
                                    .Width(40.0)
                                    .Height(40.0)
                                    .Background(Brushes.Transparent)
                                    .BorderBrush(Brushes.Transparent)
                            ))

                StackPanel().Spacing(5.0).Children(restItems |> toChildren))

    let create
        (editingRecord: R3.ReactiveProperty<WorkRecordDetailsDto option>)
        (isFormDirty: R3.ReactiveProperty<bool>)
        =
        withReactive (fun disposables _ ->
            let handleAddRestTime () =
                match editingRecord.Value with
                | Some r ->
                    let updated =
                        { r with
                            RestTimes = r.RestTimes @ [ RestRecordDetailsDto.empty ] }

                    editingRecord.Value <- Some updated
                    isFormDirty.Value <- true
                | None -> ()

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(15.0)
                .Child(
                    StackPanel()
                        .Spacing(15.0)
                        .Children(
                            Grid()
                                .ColumnDefinitions("Auto,*,Auto")
                                .Children(
                                    TextBlock()
                                        .Text("休憩")
                                        .FontSize(18.0)
                                        .FontWeightBold()
                                        .Column(0),
                                    Button()
                                        .Content("+ 追加")
                                        .OnClickHandler(fun _ _ -> handleAddRestTime ())
                                        .Column(2)
                                ),
                            Border()
                                .BorderThickness(1.0)
                                .BorderBrush(Brushes.LightGray)
                                .Padding(10.0)
                                .Child(
                                    createRestTimesContent editingRecord isFormDirty disposables
                                )
                        )
                ))
