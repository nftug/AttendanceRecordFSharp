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
        withLifecycle (fun _ self ->
            let ctx, _ = Context.require<HistoryPageContext> self

            ctx.Form
            |> toOptView (fun _ _ form ->
                let handleSetStartedAt (startedAt: DateTime option) : unit =
                    let startedAt = defaultArg startedAt form.StartedAt
                    ctx.Form.Value <- Some { form with StartedAt = startedAt }

                let handleSetEndedAt (endedAt: DateTime option) : unit =
                    ctx.Form.Value <- Some { form with EndedAt = endedAt }

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
                                              Value = ctx.Form |> R3.map (Option.map _.StartedAt)
                                              OnSetValue = handleSetStartedAt
                                              IsClearable = false },
                                        TimePickerField.create
                                            { Label = "退勤時間"
                                              BaseDate = ctx.CurrentDate
                                              Value = ctx.Form |> R3.map (Option.bind _.EndedAt)
                                              OnSetValue = handleSetEndedAt
                                              IsClearable = true }
                                    )
                            )
                    )))
