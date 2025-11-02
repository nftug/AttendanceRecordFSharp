namespace AttendanceRecord.Domain.ValueObjects.Alarms

open System
open AttendanceRecord.Domain.Entities

module RestStartAlarm =
    let private rule: AlarmRule<WorkRecord> =
        { AlarmType = AlarmType.RestStart
          ShouldTrigger =
            fun wr cfg now ->
                cfg.RestStartAlarmConfig.IsEnabled
                && wr |> WorkRecord.isActive now
                && wr |> WorkRecord.getRestDuration now = TimeSpan.Zero
                && wr |> WorkRecord.getDuration now >= cfg.RestStartAlarmConfig.BeforeStartDuration
          GetSnoozeDuration = _.RestStartAlarmConfig.SnoozeDuration }

    let createInitialAlarm () : Alarm<WorkRecord> =
        Alarm.initial rule