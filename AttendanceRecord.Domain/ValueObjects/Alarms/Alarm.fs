namespace AttendanceRecord.Domain.ValueObjects.Alarms

open System
open AttendanceRecord.Domain.Entities

type AlarmState =
    { IsTriggered: bool
      SnoozedUntil: DateTime option }

type AlarmRule<'r> =
    { AlarmType: AlarmType
      ShouldTrigger: 'r -> AppConfig -> DateTime -> bool
      GetSnoozeDuration: AppConfig -> TimeSpan }

type Alarm<'r> =
    { Rule: AlarmRule<'r>
      State: AlarmState }

module Alarm =
    let initial (rule: AlarmRule<'r>) : Alarm<'r> =
        { Rule = rule
          State =
            { IsTriggered = false
              SnoozedUntil = None } }

    let private isSnoozed now a =
        match a.State.SnoozedUntil with
        | Some t when t >= now -> true
        | _ -> false

    let tryTrigger (now: DateTime) (r: 'r) (cfg: AppConfig) (a: Alarm<'r>) : Alarm<'r> =
        if a.Rule.ShouldTrigger r cfg now && not (a |> isSnoozed now) then
            { a with
                State.IsTriggered = true
                State.SnoozedUntil = None }
        else
            { a with State.IsTriggered = false }

    let snooze (now: DateTime) (cfg: AppConfig) (a: Alarm<'r>) : Alarm<'r> =
        if a |> isSnoozed now then
            a
        else
            { a with
                State.IsTriggered = false
                State.SnoozedUntil = now.Add(a.Rule.GetSnoozeDuration cfg) |> Some }
