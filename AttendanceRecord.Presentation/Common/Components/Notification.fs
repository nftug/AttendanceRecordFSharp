namespace Avalonia.Controls.Notifications

open Avalonia
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.Builder
open Avalonia.FuncUI.DSL

[<AutoOpen>]
module WindowNotificationManager =

    let create (attrs: IAttr<WindowNotificationManager> list) : IView<WindowNotificationManager> =
        ViewBuilder.Create<WindowNotificationManager> attrs

    type WindowNotificationManager with

        static member position<'t when 't :> WindowNotificationManager>(value: NotificationPosition) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<NotificationPosition>(WindowNotificationManager.PositionProperty, value, ValueNone)

        static member maxItems<'t when 't :> WindowNotificationManager>(value: int) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<int>(WindowNotificationManager.MaxItemsProperty, value, ValueNone)

module Notification =
    let showWindowNotification (title: string) (message: string) (notificationType: NotificationType) : unit =
        tryGetControl<WindowNotificationManager> ()
        |> Option.iter (fun manager -> manager.Show(Notification(title, message, notificationType)))
