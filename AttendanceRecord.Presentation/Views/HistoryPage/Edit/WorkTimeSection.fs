namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

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

            let update (updater: WorkRecordSaveRequestDto -> WorkRecordSaveRequestDto) =
                ctx.FormCtx.Form.Value <- updater ctx.FormCtx.Form.Value

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
                                        { Label = "出勤時間" |> R3.ret
                                          BaseDate = Some ctx.CurrentDate
                                          Value = ctx.FormCtx.Form |> R3.map (Some << _.StartedAt)
                                          OnSetValue =
                                            fun v ->
                                                update (fun wr ->
                                                    { wr with
                                                        StartedAt = defaultArg v wr.StartedAt })
                                          IsClearable = false |> R3.ret },
                                    TimePickerField.create
                                        { Label = "退勤時間" |> R3.ret
                                          BaseDate = Some ctx.CurrentDate
                                          Value = ctx.FormCtx.Form |> R3.map _.EndedAt
                                          OnSetValue =
                                            fun v -> update (fun wr -> { wr with EndedAt = v })
                                          IsClearable = true |> R3.ret }
                                )
                        )
                ))
