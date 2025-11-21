namespace AttendanceRecord.Presentation.Utils

open Avalonia.Controls.Notifications

module Notification =
    let show (title: string) (message: string) (notificationType: NotificationType) : unit =
        getControlFromMainWindow<WindowNotificationManager> ()
        |> _.Show(Notification(title, message, notificationType))
