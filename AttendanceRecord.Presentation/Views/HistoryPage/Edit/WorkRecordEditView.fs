namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared

module WorkRecordEditView =
    open NXUI.Extensions
    open type NXUI.Builders
    open Avalonia.Media
    open Material.Icons

    let create () : Avalonia.Controls.Control =
        withLifecycle (fun _ self ->
            let ctx, _ = Context.require<HistoryPageContext> self

            let saveButtonEnabled =
                R3.combineLatest2 ctx.SaveMutation.IsPending ctx.FormCtx.IsFormDirty
                |> R3.map (fun (isSaving, dirty) -> not isSaving && dirty)

            let deleteButtonEnabled =
                R3.combineLatest2 ctx.FormCtx.Form ctx.DeleteMutation.IsPending
                |> R3.map (fun (f, isDeleting) -> f.Id.IsSome && not isDeleting)

            Grid()
                .RowDefinitions("*,Auto")
                .Margin(20.0)
                .RowSpacing(20.0)
                .Children(
                    Grid()
                        .RowDefinitions("Auto,Auto,Auto,*")
                        .RowSpacing(20.0)
                        .Children(
                            TextBlock()
                                .Text(
                                    ctx.CurrentDate
                                    |> R3.map _.ToString("yyyy/MM/dd (ddd)")
                                    |> asBinding
                                )
                                .FontSize(28.0)
                                .FontWeightBold()
                                .Row(0),
                            WorkStatusSummarySection.create () |> _.Row(1),
                            WorkTimeSection.create () |> _.Row(2),
                            RestTimeSection.create () |> _.Row(3)
                        )
                        .Row(0),
                    Grid()
                        .ColumnDefinitions("Auto,*,Auto,Auto")
                        .ColumnSpacing(10.0)
                        .Children(
                            Button()
                                .Content(
                                    MaterialIconLabel.create
                                        { Kind = MaterialIconKind.Delete |> R3.ret
                                          Label = "削除" |> R3.ret
                                          Spacing = None |> R3.ret }
                                )
                                .OnClickHandler(fun _ _ -> ctx.DeleteMutation.Mutate())
                                .Width(100.0)
                                .Height(35.0)
                                .IsEnabled(deleteButtonEnabled |> asBinding)
                                .Background(Brushes.DarkRed)
                                .Foreground(Brushes.White)
                                .Column(0),
                            Button()
                                .Content(
                                    MaterialIconLabel.create
                                        { Kind = MaterialIconKind.Refresh |> R3.ret
                                          Label = "リセット" |> R3.ret
                                          Spacing = None |> R3.ret }
                                )
                                .OnClickHandler(fun _ _ -> ctx.FormCtx.ResetForm None)
                                .Width(100.0)
                                .Height(35.0)
                                .IsEnabled(ctx.FormCtx.IsFormDirty |> asBinding)
                                .Column(2),
                            Button()
                                .Content(
                                    MaterialIconLabel.create
                                        { Kind = MaterialIconKind.ContentSave |> R3.ret
                                          Label = "保存" |> R3.ret
                                          Spacing = None |> R3.ret }
                                )
                                .OnClickHandler(fun _ _ -> ctx.SaveMutation.Mutate())
                                .Width(100.0)
                                .Height(35.0)
                                .IsEnabled(saveButtonEnabled |> asBinding)
                                .Column(3)
                            |> Colors.setAccentColorBackground
                        )
                        .Row(1)
                ))
