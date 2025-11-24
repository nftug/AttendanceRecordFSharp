namespace AttendanceRecord.Presentation.Views.SettingsPage

open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared

module SettingsActionsView =
    open type NXUI.Builders
    open NXUI.Extensions
    open Material.Icons

    let create () =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<SettingsPageContext> self

            let saveButtonEnabled =
                R3.combineLatest2 ctx.FormCtx.IsFormDirty ctx.SaveMutation.IsPending
                |> R3.map (fun (dirty, isSaving) -> dirty && not isSaving)

            StackPanel()
                .OrientationHorizontal()
                .HorizontalAlignmentRight()
                .Spacing(10.0)
                .Children(
                    Button()
                        .Content(
                            MaterialIconLabel.create
                                { Kind = MaterialIconKind.Refresh |> R3.ret
                                  Label = "リセット" |> R3.ret
                                  Spacing = None |> R3.ret }
                        )
                        .Width(100.0)
                        .Height(35.0)
                        .OnClickHandler(fun _ _ -> ctx.FormCtx.ResetForm None)
                        .IsEnabled(ctx.FormCtx.IsFormDirty |> asBinding),
                    Button()
                        .Content(
                            MaterialIconLabel.create
                                { Kind = MaterialIconKind.ContentSave |> R3.ret
                                  Label = "保存" |> R3.ret
                                  Spacing = None |> R3.ret }
                        )
                        .Width(100.0)
                        .Height(35.0)
                        .OnClickHandler(fun _ _ -> ctx.SaveMutation.Mutate())
                        .IsEnabled(saveButtonEnabled |> asBinding)
                    |> Colors.setAccentColorBackground
                ))
