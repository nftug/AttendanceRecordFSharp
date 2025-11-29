namespace AttendanceRecord.Presentation.Views.SettingsPage.Context

open System.Threading
open R3
open FsToolkit.ErrorHandling
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.UseCases.AppConfig
open AttendanceRecord.Presentation.Utils

type SettingsPageContext =
    { FormCtx: FormContext<AppConfigSaveRequestDto, AppConfigError>
      ConfirmDiscard: CancellationToken -> Tasks.Task<bool>
      SaveMutation: UseMutationResult<unit, unit, AppConfigError list> }

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
                            Dialog.show
                                { Title = "変更の確認"
                                  Message = "保存されていない変更があります。\n変更を破棄してもよろしいですか？"
                                  Buttons = YesNoButton(Some "破棄", Some "キャンセル") }
                                (Some ct)
                            |> Task.map ((=) YesResult)
                    else
                        return true
                })

        let saveMutation =
            useMutation disposables (fun () ct ->
                task {
                    let request = formCtx.Form.CurrentValue

                    match! props.SaveAppConfig.Handle request ct with
                    | Ok() ->
                        Notification.show
                            { Title = "保存完了"
                              Message = "アプリ設定を保存しました。"
                              NotificationType = NotificationType.Success }

                        return Ok()
                    | Error errors ->
                        formCtx.Errors.Value <- errors

                        match errors |> AppConfigErrors.chooseVariants with
                        | msgs when not (List.isEmpty msgs) ->
                            Notification.show
                                { Title = "エラーが発生しました"
                                  Message = System.String.Join("\n", msgs)
                                  NotificationType = NotificationType.Error }
                        | _ -> ()

                        return Error errors
                })

        { FormCtx = formCtx
          ConfirmDiscard = confirmDiscard
          SaveMutation = saveMutation }
