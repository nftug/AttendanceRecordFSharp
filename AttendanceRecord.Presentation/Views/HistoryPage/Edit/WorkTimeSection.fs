namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open System
open Avalonia.Media
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared

module WorkTimeSection =
    open NXUI.Extensions
    open type NXUI.Builders

    let create () =
        withReactive (fun disposables self ->
            let startedAt = R3.property (None: DateTime option) |> R3.disposeWith disposables
            let endedAt = R3.property (None: DateTime option) |> R3.disposeWith disposables
            let ctx, _ = HistoryPageContextProvider.require self

            ctx.Form
            |> R3.subscribe (fun rOpt ->
                match rOpt with
                | Some r ->
                    startedAt.Value <- Some r.StartedAt
                    endedAt.Value <- r.EndedAt
                | None -> ())
            |> disposables.Add

            startedAt
            |> R3.combineLatest2 endedAt (fun sa ea -> sa, ea)
            |> R3.subscribe (fun (s, e) ->
                match ctx.Form.Value with
                | Some r ->
                    ctx.Form.Value <-
                        Some
                            { r with
                                StartedAt = defaultArg s r.StartedAt
                                EndedAt = e }
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
                                          BaseDate = ctx.CurrentDate
                                          SelectedDateTime = startedAt
                                          IsDirty = Some ctx.IsFormDirty
                                          IsClearable = false },
                                    TimePickerField.create
                                        { Label = "退勤時間"
                                          BaseDate = ctx.CurrentDate
                                          SelectedDateTime = endedAt
                                          IsDirty = Some ctx.IsFormDirty
                                          IsClearable = true }
                                )
                        )
                ))
