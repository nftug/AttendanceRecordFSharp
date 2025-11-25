namespace AttendanceRecord.Presentation.Views.Common

type MessageBoxButtons =
    | OkButton of string option
    | YesNoButton of string option * string option
    | YesNoCancelButton of string option * string option * string option

type MessageBoxResult =
    | OkResult
    | YesResult
    | NoResult
    | CancelResult

type MessageBoxProps =
    { Title: string
      Message: string
      Buttons: MessageBoxButtons }

module MessageBox =
    open System.Threading
    open System.Threading.Tasks
    open FluentAvalonia.UI.Controls
    open AttendanceRecord.Presentation.Utils

    let show (props: MessageBoxProps) (ct: CancellationToken option) : Task<MessageBoxResult> =
        task {
            let window = getMainWindow ()
            window.Show()
            window.Activate()

            let dialog = ContentDialog(Title = props.Title, Content = props.Message)

            match props.Buttons with
            | OkButton ok -> dialog.CloseButtonText <- defaultArg ok "OK"
            | YesNoButton(yes, no) ->
                dialog.PrimaryButtonText <- defaultArg yes "はい"
                dialog.CloseButtonText <- defaultArg no "いいえ"
            | YesNoCancelButton(yes, no, cancel) ->
                dialog.PrimaryButtonText <- defaultArg yes "はい"
                dialog.SecondaryButtonText <- defaultArg no "いいえ"
                dialog.CloseButtonText <- defaultArg cancel "キャンセル"

            let! result = dialog.ShowAsync()

            match ct with
            | Some cancellationToken -> cancellationToken.ThrowIfCancellationRequested()
            | None -> ()

            return
                match props.Buttons, result with
                | OkButton _, _ -> OkResult
                | _, ContentDialogResult.Primary -> YesResult
                | _, ContentDialogResult.Secondary -> NoResult
                | _ -> CancelResult
        }
