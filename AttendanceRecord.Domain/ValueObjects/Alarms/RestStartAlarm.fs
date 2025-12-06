namespace AttendanceRecord.Domain.ValueObjects.Alarms

open System
open AttendanceRecord.Domain.Entities

type RestStartAlarm = Alarm<WorkRecord>

module RestStartAlarm =
   let private rule: AlarmRule<WorkRecord> =
      { AlarmType = RestStartAlarm
        ShouldTrigger =
         fun wr cfg now ->
            cfg.RestStartAlarm.IsEnabled
            && wr |> WorkRecord.isActive now
            && wr |> WorkRecord.getRestDuration now RegularRest = TimeSpan.Zero
            && wr |> WorkRecord.getWorkDuration now >= cfg.RestStartAlarm.BeforeStartDuration
        GetSnoozeDuration = _.RestStartAlarm.SnoozeDuration }

   let createInitialAlarm () : RestStartAlarm = Alarm.initial rule
