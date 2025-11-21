namespace AttendanceRecord.Presentation.Views.Application

open R3
open Avalonia.Controls
open Avalonia.Controls.Notifications
open AttendanceRecord.Application.Services
open AttendanceRecord.Domain.ValueObjects.Alarms
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Shared

module AlarmHost =
    let start (alarmService: AlarmService) (disposables: CompositeDisposable) =
        alarmService.AlarmTriggered
        |> R3.subscribe (fun alarmType ->
            let window = getMainWindow ()
            window.Show()
            window.WindowState <- WindowState.Normal

            let title =
                match alarmType with
                | AlarmType.WorkEnd -> "勤務終了アラーム"
                | AlarmType.RestStart -> "休憩開始アラーム"
                | _ -> ""

            let message =
                match alarmType with
                | AlarmType.WorkEnd -> "まもなく勤務終了時刻です。"
                | AlarmType.RestStart -> "休憩開始時間になりました。"
                | _ -> ""

            Notification.show title message NotificationType.Information

            task {
                let! shouldSnooze =
                    MessageBox.show
                        { Title = title
                          Message = message + "\nアラームをスヌーズしますか？"
                          Buttons = MessageBoxButtons.OkCancel
                          OkContent = Some "スヌーズ"
                          CancelContent = Some "閉じる" }
                        None

                if shouldSnooze then
                    alarmService.SnoozeAlarm alarmType
                    Notification.show title "アラームをスヌーズしました。" NotificationType.Information
            }
            |> ignore)
        |> disposables.Add
