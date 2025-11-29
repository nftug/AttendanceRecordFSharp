namespace AttendanceRecord.Application.Services

open System
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects.Alarms

type AlarmService =
    { AlarmTriggered: Observable<AlarmType>
      SnoozeAlarm: AlarmType -> unit }

type AlarmServiceDependencies =
    { StatusStore: WorkStatusStore
      AppConfig: ReadOnlyReactiveProperty<AppConfig>
      Disposables: CompositeDisposable }

module AlarmService =
    type private AlarmTriggerState =
        { AlarmType: AlarmType
          IsTriggered: bool }

    let create (deps: AlarmServiceDependencies) : AlarmService =
        let workEndAlarm =
            R3.property (WorkEndAlarm.createInitialAlarm ())
            |> R3.disposeWith deps.Disposables

        let restStartAlarm =
            R3.property (RestStartAlarm.createInitialAlarm ())
            |> R3.disposeWith deps.Disposables

        let alarmTriggered = R3.command<AlarmType> () |> R3.disposeWith deps.Disposables

        R3.combineLatest2 deps.StatusStore.WorkRecord deps.AppConfig
        |> R3.subscribe (fun (workRecordOption, appConfig) ->
            match workRecordOption with
            | Some record ->
                let now = DateTime.Now

                workEndAlarm.Value <- workEndAlarm.Value |> Alarm.tryTrigger now record appConfig

                restStartAlarm.Value <-
                    restStartAlarm.Value |> Alarm.tryTrigger now record appConfig
            | None -> ())
        |> deps.Disposables.Add

        [ workEndAlarm; restStartAlarm ]
        |> List.iter (fun alarm ->
            alarm
            |> R3.map (fun a ->
                { AlarmType = a.Rule.AlarmType
                  IsTriggered = a.State.IsTriggered })
            |> R3.distinctUntilChanged
            |> R3.filter _.IsTriggered
            |> R3.subscribe (fun t -> alarmTriggered.Execute t.AlarmType)
            |> deps.Disposables.Add)

        let snoozeAlarm (alarmType: AlarmType) =
            let now = DateTime.Now
            let cfg = deps.AppConfig.CurrentValue

            match alarmType with
            | WorkEndAlarm -> workEndAlarm.Value <- workEndAlarm.Value |> Alarm.snooze now cfg
            | RestStartAlarm -> restStartAlarm.Value <- restStartAlarm.Value |> Alarm.snooze now cfg

        { AlarmTriggered = alarmTriggered
          SnoozeAlarm = snoozeAlarm }
