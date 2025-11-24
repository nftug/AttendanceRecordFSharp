namespace AttendanceRecord.Presentation.Views.SettingsPage.Context

open System.Threading
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.UseCases.AppConfig
open AttendanceRecord.Presentation.Utils

type SettingsPageContext =
    { FormCtx: FormContext<AppConfigSaveRequestDto>
      ConfirmDiscard: CancellationToken -> Tasks.Task<bool> }

type SettingsPageContextProps =
    { AppConfig: Observable<AppConfigDto>
      SaveAppConfig: SaveAppConfig }

module SettingsPageContext =
    let create
        (props: SettingsPageContextProps)
        (disposables: CompositeDisposable)
        : SettingsPageContext =
        let appConfig = props.AppConfig |> R3.readonly None |> R3.disposeWith disposables

        let formCtx = FormContext.create AppConfigSaveRequestDto.empty disposables

        appConfig
        |> R3.subscribe (fun config ->
            let request = AppConfigSaveRequestDto.fromResponse config
            formCtx.ResetForm(Some request))
        |> disposables.Add

        let confirmDiscard _ : Tasks.Task<bool> =
            invokeTask disposables (fun ct ->
                task {
                    if formCtx.IsFormDirty.CurrentValue then
                        return!
                            MessageBox.show
                                { Title = "変更の確認"
                                  Message = "保存されていない変更があります。\n変更を破棄してもよろしいですか？"
                                  Buttons = MessageBoxButtons.OkCancel
                                  OkContent = Some "破棄"
                                  CancelContent = Some "キャンセル" }
                                (Some ct)
                    else
                        return true
                })

        { FormCtx = formCtx
          ConfirmDiscard = confirmDiscard }
