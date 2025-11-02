namespace AttendanceRecord.Application.Services

open System
open R3
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects.Alarms

type private AlarmTriggerState =
    { AlarmType: AlarmType
      IsTriggered: bool }

type AlarmService(currentStatusStore: CurrentStatusStore, getAppConfig: unit -> AppConfig) =
    let disposable = new CompositeDisposable()

    let workEndAlarm =
        new ReactiveProperty<WorkEndAlarm>(WorkEndAlarm.createInitialAlarm ())

    let restStartAlarm =
        new ReactiveProperty<RestStartAlarm>(RestStartAlarm.createInitialAlarm ())

    let alarmTriggered = new ReactiveCommand<AlarmType>()

    do
        workEndAlarm.AddTo disposable |> ignore
        restStartAlarm.AddTo disposable |> ignore
        alarmTriggered.AddTo disposable |> ignore

        currentStatusStore.WorkRecord
            .Subscribe(fun wr ->
                match wr with
                | Some record ->
                    let now = DateTime.Now
                    workEndAlarm.Value <- workEndAlarm.Value |> Alarm.tryTrigger now record (getAppConfig ())
                    restStartAlarm.Value <- restStartAlarm.Value |> Alarm.tryTrigger now record (getAppConfig ())
                | None -> ())
            .AddTo(disposable)
        |> ignore

        Observable
            .Merge(
                workEndAlarm.Select(fun a ->
                    { AlarmType = a.Rule.AlarmType
                      IsTriggered = a.State.IsTriggered }),
                restStartAlarm.Select(fun a ->
                    { AlarmType = a.Rule.AlarmType
                      IsTriggered = a.State.IsTriggered })
            )
            .Where(fun s -> s.IsTriggered)
            .Delay(TimeSpan.FromMilliseconds 100.0)
            .Subscribe(fun s -> alarmTriggered.Execute(s.AlarmType) |> ignore)
            .AddTo(disposable)
        |> ignore

    member _.AlarmTriggered: Observable<AlarmType> = alarmTriggered

    member _.SnoozeAlarm(alarmType: AlarmType) : unit =
        let now = DateTime.Now
        let cfg = getAppConfig ()

        match alarmType with
        | AlarmType.WorkEnd -> workEndAlarm.Value <- workEndAlarm.Value |> Alarm.snooze now cfg
        | AlarmType.RestStart -> restStartAlarm.Value <- restStartAlarm.Value |> Alarm.snooze now cfg

    interface IDisposable with
        member _.Dispose() = disposable.Dispose()
