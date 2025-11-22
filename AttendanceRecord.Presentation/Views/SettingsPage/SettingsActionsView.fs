namespace AttendanceRecord.Presentation.Views.SettingsPage

open AttendanceRecord.Application.UseCases.AppConfig
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared

type SettingsActionsViewProps = { SaveAppConfig: SaveAppConfig }

[<AutoOpen>]
module private SettingsActionsViewHelpers =
    open AttendanceRecord.Application.Dtos.Requests
    open System.Threading

    let handleClickSave
        (handle: AppConfigSaveRequestDto -> CancellationToken -> Tasks.Task<Result<unit, string>>)
        (ctx: SettingsPageContext)
        (disposables: R3.CompositeDisposable)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                let request = ctx.Form.CurrentValue
                let! result = handle request ct

                match result with
                | Ok() ->
                    Notification.show
                        { Title = "保存完了"
                          Message = "アプリ設定を保存しました。"
                          NotificationType = NotificationType.Success }
                | Error e ->
                    Notification.show
                        { Title = "保存エラー"
                          Message = $"アプリ設定の保存に失敗しました: {e}"
                          NotificationType = NotificationType.Error }
            })
        |> ignore

module SettingsActionsView =
    open type NXUI.Builders
    open NXUI.Extensions
    open Material.Icons

    let create (props: SettingsActionsViewProps) =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<SettingsPageContext> self
            let saveMutation = useMutation disposables props.SaveAppConfig.Handle

            let isFormDirty =
                R3.combineLatest2 ctx.Form ctx.DefaultForm
                |> R3.map (fun (form, def) -> form <> def)

            let saveButtonEnabled =
                R3.combineLatest2 isFormDirty saveMutation.IsPending
                |> R3.map (fun (dirty, isSaving) -> dirty && not isSaving)

            StackPanel()
                .OrientationHorizontal()
                .HorizontalAlignmentRight()
                .Spacing(10.0)
                .Children(
                    Button()
                        .Content(MaterialIconLabel.create MaterialIconKind.Refresh "リセット")
                        .Width(100.0)
                        .Height(35.0)
                        .OnClickHandler(fun _ _ -> ctx.Form.Value <- ctx.DefaultForm.CurrentValue)
                        .IsEnabled(isFormDirty |> asBinding),
                    Button()
                        .Content(MaterialIconLabel.create MaterialIconKind.ContentSave "保存")
                        .Width(100.0)
                        .Height(35.0)
                        .OnClickHandler(fun _ _ ->
                            handleClickSave saveMutation.MutateTask ctx disposables)
                        .IsEnabled(saveButtonEnabled |> asBinding)
                    |> Colors.setAccentColorBackground
                ))
