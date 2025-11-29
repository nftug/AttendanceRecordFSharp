namespace AttendanceRecord.Domain.ValueObjects.Alarms

open AttendanceRecord.Domain.Entities

type WorkEndAlarm = Alarm<WorkRecord>

module WorkEndAlarm =
    let private rule: AlarmRule<WorkRecord> =
        { AlarmType = WorkEndAlarm
          ShouldTrigger =
            fun wr cfg now ->
                cfg.WorkEndAlarm.IsEnabled
                && wr |> WorkRecord.isActive now
                && wr
                   |> WorkRecord.getOvertimeDuration
                       now
                       (cfg.StandardWorkTime |> StandardWorkTime.value)
                   >= -cfg.WorkEndAlarm.BeforeEndDuration
          GetSnoozeDuration = _.WorkEndAlarm.SnoozeDuration }

    let createInitialAlarm () : WorkEndAlarm = Alarm.initial rule
