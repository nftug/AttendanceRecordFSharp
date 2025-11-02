namespace AttendanceRecord.Domain.ValueObjects.Alarms

open AttendanceRecord.Domain.Entities

type WorkEndAlarm = Alarm<WorkRecord>

module WorkEndAlarm =
    let private rule: AlarmRule<WorkRecord> =
        { AlarmType = AlarmType.WorkEnd
          ShouldTrigger =
            fun wr cfg now ->
                cfg.WorkEndAlarmConfig.IsEnabled
                && wr |> WorkRecord.isActive now
                && wr |> WorkRecord.getOvertimeDuration now cfg.StandardWorkTime
                   >= -cfg.WorkEndAlarmConfig.BeforeEndDuration
          GetSnoozeDuration = _.WorkEndAlarmConfig.SnoozeDuration }

    let createInitialAlarm () : WorkEndAlarm = Alarm.initial rule
