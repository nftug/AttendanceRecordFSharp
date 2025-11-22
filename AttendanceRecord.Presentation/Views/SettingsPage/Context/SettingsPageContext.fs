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
    { Form: ReactiveProperty<AppConfigSaveRequestDto>
      DefaultForm: ReadOnlyReactiveProperty<AppConfigSaveRequestDto>
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

        let form = R3.property AppConfigSaveRequestDto.empty |> R3.disposeWith disposables

        let defaultForm = R3.property form.CurrentValue |> R3.disposeWith disposables

        appConfig
        |> R3.subscribe (fun config ->
            let dto = AppConfigSaveRequestDto.fromResponse config
            form.Value <- dto
            defaultForm.Value <- dto)
        |> disposables.Add

        let confirmDiscard (ct: CancellationToken) : Tasks.Task<bool> =
            invokeTask disposables (fun _ ->
                task {
                    if form.CurrentValue <> defaultForm.CurrentValue then
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

        { Form = form
          DefaultForm = defaultForm :> ReadOnlyReactiveProperty<_>
          ConfirmDiscard = confirmDiscard }
