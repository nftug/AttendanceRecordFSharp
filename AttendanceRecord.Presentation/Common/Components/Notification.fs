namespace Avalonia.Controls.Notifications

open Avalonia.Controls

module Notification =
    let mutable private windowNotificationManager: WindowNotificationManager option =
        None

    let initWindowNotificationManager (window: Window) (config: WindowNotificationManager -> unit) : unit =
        let manager = WindowNotificationManager window
        config manager
        windowNotificationManager <- Some manager

    let showWindowNotification (title: string) (message: string) (notificationType: NotificationType) : unit =
        match windowNotificationManager with
        | Some manager -> manager.Show(Notification(title, message, notificationType))
        | None -> invalidOp "WindowNotificationManager is not initialized."
