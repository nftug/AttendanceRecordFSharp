namespace AttendanceRecord.Presentation.Views.SettingsPage.Context

open System.Threading
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.UseCases.AppConfig
open AttendanceRecord.Presentation.Utils

type SettingsPageContext =
    { Form: ReactiveProperty<AppConfigSaveRequestDto>
      DefaultForm: ReadOnlyReactiveProperty<AppConfigSaveRequestDto>
      ReloadAfterSave: unit -> unit
      ConfirmDiscard: CancellationToken -> Tasks.Task<bool> }

type SettingsPageContextProps =
    { GetAppConfig: GetAppConfig
      SaveAppConfig: SaveAppConfig }

module SettingsPageContext =
    let create
        (props: SettingsPageContextProps)
        (disposables: CompositeDisposable)
        : SettingsPageContext =
        let form = R3.property AppConfigSaveRequestDto.empty |> R3.disposeWith disposables

        let defaultForm = R3.property form.CurrentValue |> R3.disposeWith disposables

        let loadAppConfig () : unit =
            let appConfig = props.GetAppConfig.Handle()
            form.Value <- AppConfigSaveRequestDto.fromResponse appConfig
            defaultForm.Value <- form.CurrentValue

        let confirmDiscard (ct: CancellationToken) : Tasks.Task<bool> =
            invokeTask disposables (fun _ ->
                task {
                    if form.CurrentValue <> defaultForm.CurrentValue then
                        return!
                            MessageBox.show
                                { Title = "変更の確認"
                                  Message = "設定に未保存の変更があります。\n変更を破棄してもよろしいですか？"
                                  Buttons = MessageBoxButtons.OkCancel
                                  OkContent = Some "破棄"
                                  CancelContent = Some "キャンセル" }
                                (Some ct)
                    else
                        return true
                })

        loadAppConfig ()

        { Form = form
          DefaultForm = defaultForm :> ReadOnlyReactiveProperty<_>
          ReloadAfterSave = loadAppConfig
          ConfirmDiscard = confirmDiscard }
