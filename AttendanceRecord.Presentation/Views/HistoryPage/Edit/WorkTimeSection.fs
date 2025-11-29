namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open Avalonia.Media
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Shared

module WorkTimeSection =
    open NXUI.Extensions
    open type NXUI.Builders

    let create () =
        withLifecycle (fun _ self ->
            let ctx, _ = Context.require<HistoryPageContext> self

            let update (updater: WorkRecordSaveRequestDto -> WorkRecordSaveRequestDto) =
                ctx.FormCtx.Form.Value <- updater ctx.FormCtx.Form.Value

                ctx.FormCtx.Errors.Value <-
                    ctx.FormCtx.Errors.Value |> List.filter (_.IsWorkDurationError >> not)

            Border()
                .BorderThickness(1.0)
                .BorderBrush(Brushes.Gray)
                .Padding(15.0)
                .Child(
                    StackPanel()
                        .Spacing(15.0)
                        .Children(
                            TextBlock().Text("出退勤").FontSize(18.0).FontWeightBold(),
                            TimeDurationPicker.create
                                { StartedAt = ctx.FormCtx.Form |> R3.map _.StartedAt
                                  EndedAt = ctx.FormCtx.Form |> R3.map _.EndedAt
                                  OnStartedAtChanged =
                                    fun v ->
                                        update (fun wr ->
                                            { wr with
                                                StartedAt = defaultArg v wr.StartedAt })
                                  OnEndedAtChanged =
                                    fun v -> update (fun wr -> { wr with EndedAt = v })
                                  Errors =
                                    ctx.FormCtx.Errors
                                    |> R3.map WorkRecordErrors.chooseDurationOrVariants
                                  Spacing = Some 15.0 }
                        )
                ))
