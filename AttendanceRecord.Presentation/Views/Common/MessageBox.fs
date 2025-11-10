namespace AttendanceRecord.Presentation.Views.Common

open AttendanceRecord.Presentation.Utils

type MessageBoxButtons =
    | Ok = 0
    | OkCancel = 1

type MessageBoxProps =
    { Title: string
      Message: string
      OkContent: obj option
      CancelContent: obj option
      Buttons: MessageBoxButtons }

module private MessageBoxView =
    open NXUI.Extensions
    open type NXUI.Builders
    open System.Windows.Input

    let create (props: MessageBoxProps) (closeCommand: ICommand) =
        let createButton onClick content =
            Button()
                .Content(content)
                .MinWidth(80.0)
                .Height(35.0)
                .OnClickHandler(fun _ _ -> onClick ())

        let okButton =
            match props.OkContent with
            | Some c -> c
            | None -> "OK"
            |> createButton (fun () -> closeCommand.Execute true)

        let cancelButton =
            match props.CancelContent with
            | Some c -> c
            | None -> "キャンセル"
            |> createButton (fun () -> closeCommand.Execute false)

        StackPanel()
            .Margin(20.0)
            .Spacing(20.0)
            .MinWidth(300.0)
            .MaxWidth(400.0)
            .Children(
                TextBlock()
                    .Text(props.Title)
                    .FontSize(20.0)
                    .FontWeightBold()
                    .TextWrappingWrap()
                    .Margin(0, 0, 0, 10.0),
                TextBlock()
                    .Text(props.Message)
                    .FontSize(16.0)
                    .TextWrappingWrap()
                    .Margin(0, 0, 0, 20.0),
                StackPanel()
                    .OrientationHorizontal()
                    .HorizontalAlignmentRight()
                    .Margin(0, 10.0, 0, 0)
                    .Spacing(10.0)
                    .Children(
                        match props.Buttons with
                        | MessageBoxButtons.OkCancel -> [ cancelButton; okButton ]
                        | _ -> [ okButton ]
                        |> toChildren
                    )
            )

module MessageBox =
    open DialogHostAvalonia
    open System.Threading
    open System.Threading.Tasks

    let show (props: MessageBoxProps) (ct: CancellationToken option) : Task<bool> =
        task {
            let dialogHost = getControlFromMainWindow<DialogHost> ()

            let! result = MessageBoxView.create props dialogHost.CloseDialogCommand |> DialogHost.Show

            match ct with
            | Some cancellationToken -> cancellationToken.ThrowIfCancellationRequested()
            | None -> ()

            return result :?> bool
        }
