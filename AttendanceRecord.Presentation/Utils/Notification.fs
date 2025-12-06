namespace AttendanceRecord.Presentation.Utils

open Avalonia.Controls

type NotificationType =
   | InformationNotification
   | SuccessNotification
   | WarningNotification
   | ErrorNotification

type NotificationProps =
   { Title: string
     Message: string
     NotificationType: NotificationType }

module Notification =
   let show (props: NotificationProps) : unit =
      let notificationType =
         match props.NotificationType with
         | InformationNotification -> Notifications.NotificationType.Information
         | SuccessNotification -> Notifications.NotificationType.Success
         | WarningNotification -> Notifications.NotificationType.Warning
         | ErrorNotification -> Notifications.NotificationType.Error

      getControlFromMainWindow<Notifications.WindowNotificationManager> ()
      |> _.Show(Notifications.Notification(props.Title, props.Message, notificationType))
