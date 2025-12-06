namespace AttendanceRecord.Presentation.Views.Application

open R3
open Avalonia.Controls
open AttendanceRecord.Application.Services
open AttendanceRecord.Domain.ValueObjects.Alarms
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Shared

module AlarmHost =
   let create (alarmService: AlarmService) =
      withLifecycle (fun disposables _ ->
         alarmService.AlarmTriggered
         |> R3.subscribe (fun alarmType ->
            let title =
               match alarmType with
               | WorkEndAlarm -> "勤務終了アラーム"
               | RestStartAlarm -> "休憩開始アラーム"

            let message =
               match alarmType with
               | WorkEndAlarm -> "まもなく勤務終了時刻です。"
               | RestStartAlarm -> "休憩開始時間になりました。"

            Notification.show
               { Title = title
                 Message = message
                 NotificationType = WarningNotification }

            task {
               let! result =
                  Dialog.showAsWindow
                     { Title = title
                       Message = message + "\nアラームをスヌーズしますか？"
                       Buttons = YesNoButton(Some "スヌーズ", Some "閉じる") }
                     None

               if result = YesResult then
                  alarmService.SnoozeAlarm alarmType

                  Notification.show
                     { Title = title
                       Message = "アラームをスヌーズしました。"
                       NotificationType = InformationNotification }
            }
            |> ignore)
         |> disposables.Add

         Control())
