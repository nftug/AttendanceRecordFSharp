namespace AttendanceRecord.Application.Services

open System
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects.Alarms

type private AlarmTriggerState =
    { AlarmType: AlarmType
      IsTriggered: bool }

type AlarmService(currentStatusStore: CurrentStatusStore, getAppConfig: unit -> AppConfig) =
    let disposable = new CompositeDisposable()

    let workEndAlarm =
        R3.property (WorkEndAlarm.createInitialAlarm ()) |> R3.disposeWith disposable

    let restStartAlarm =
        R3.property (RestStartAlarm.createInitialAlarm ()) |> R3.disposeWith disposable

    let alarmTriggered = R3.command<AlarmType> () |> R3.disposeWith disposable

    do
        currentStatusStore.WorkRecord
        |> R3.subscribe (fun wr ->
            match wr with
            | Some record ->
                let now = DateTime.Now

                workEndAlarm.Value <-
                    workEndAlarm.Value |> Alarm.tryTrigger now record (getAppConfig ())

                restStartAlarm.Value <-
                    restStartAlarm.Value |> Alarm.tryTrigger now record (getAppConfig ())
            | None -> ())
        |> disposable.Add

        [ workEndAlarm; restStartAlarm ]
        |> List.iter (fun alarm ->
            alarm
            |> R3.map (fun a ->
                { AlarmType = a.Rule.AlarmType
                  IsTriggered = a.State.IsTriggered })
            |> R3.distinctUntilChanged
            |> R3.filter _.IsTriggered
            |> R3.subscribe (fun t -> alarmTriggered.Execute t.AlarmType)
            |> disposable.Add)

    member _.AlarmTriggered: Observable<AlarmType> = alarmTriggered

    member _.SnoozeAlarm(alarmType: AlarmType) : unit =
        let now = DateTime.Now
        let cfg = getAppConfig ()

        match alarmType with
        | AlarmType.WorkEnd -> workEndAlarm.Value <- workEndAlarm.Value |> Alarm.snooze now cfg
        | AlarmType.RestStart ->
            restStartAlarm.Value <- restStartAlarm.Value |> Alarm.snooze now cfg
        | _ -> ()

    interface IDisposable with
        member _.Dispose() = disposable.Dispose()
