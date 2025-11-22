namespace AttendanceRecord.Presentation.Utils

open Avalonia.Controls

type NotificationType =
    | Information = 0
    | Success = 1
    | Warning = 2
    | Error = 3

type NotificationProps =
    { Title: string
      Message: string
      NotificationType: NotificationType }

module Notification =
    let show (props: NotificationProps) : unit =
        let notificationType =
            match props.NotificationType with
            | NotificationType.Information -> Notifications.NotificationType.Information
            | NotificationType.Success -> Notifications.NotificationType.Success
            | NotificationType.Warning -> Notifications.NotificationType.Warning
            | NotificationType.Error -> Notifications.NotificationType.Error
            | _ -> Notifications.NotificationType.Information

        getControlFromMainWindow<Notifications.WindowNotificationManager> ()
        |> _.Show(Notifications.Notification(props.Title, props.Message, notificationType))
