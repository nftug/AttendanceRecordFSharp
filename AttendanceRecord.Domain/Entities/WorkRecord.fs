namespace AttendanceRecord.Domain.Entities

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.ValueObjects

type WorkRecord =
    { Id: Guid
      Duration: TimeDuration
      RestTimes: RestRecord list }

module WorkRecord =
    let hydrate (id: Guid) (duration: TimeDuration) (restTimes: RestRecord list) : WorkRecord =
        { Id = id
          Duration = duration
          RestTimes = restTimes |> RestRecord.getSortedList }

    // Status getters
    let getStartedAt (record: WorkRecord) : DateTime =
        record.Duration |> TimeDuration.getStartedAt

    let getEndedAt (record: WorkRecord) : DateTime option =
        record.Duration |> TimeDuration.getEndedAt

    let getRestDuration (record: WorkRecord) : TimeSpan =
        record.RestTimes |> RestRecord.getDurationOfList

    let getDuration (record: WorkRecord) : TimeSpan =
        TimeDuration.getDuration record.Duration - getRestDuration record

    let getOvertimeDuration (standardWorkTime: TimeSpan) (record: WorkRecord) : TimeSpan =
        getDuration record - standardWorkTime

    let hasDate (date: DateTime) (record: WorkRecord) : bool = (getStartedAt record).Date = date.Date

    let isTodays (record: WorkRecord) : bool = hasDate DateTime.Now.Date record

    let isActive (record: WorkRecord) : bool = TimeDuration.isActive record.Duration

    let isResting (record: WorkRecord) : bool =
        record.RestTimes |> RestRecord.isRestingOfList

    let isWorking (record: WorkRecord) : bool =
        isActive record && not (isResting record)

    // Factory methods
    let create (duration: TimeDuration) (restTimes: RestRecord list) : WorkRecord =
        { Id = Guid.NewGuid()
          Duration = duration
          RestTimes = restTimes |> RestRecord.getSortedList }

    let update (newDuration: TimeDuration) (newRestTimes: RestRecord list) (record: WorkRecord) : WorkRecord =
        { record with
            Duration = newDuration
            RestTimes = newRestTimes |> RestRecord.getSortedList }

    let createStart () : WorkRecord =
        { Id = Guid.NewGuid()
          Duration = TimeDuration.createStart ()
          RestTimes = [] }

    let toggleRest (record: WorkRecord) : Result<WorkRecord, string> =
        if not (isActive record) then
            Error "Work time is not active"
        else
            let restTimes = record.RestTimes |> RestRecord.toggleOfList
            Ok { record with RestTimes = restTimes }

    let toggleWork (record: WorkRecord) : Result<WorkRecord, string> =
        result {
            if not (isTodays record) then
                return! Error "Can only toggle work for today's record"
            else if isActive record then
                // End work
                let! endedDuration = TimeDuration.createEnd record.Duration

                return
                    { record with
                        Duration = endedDuration
                        RestTimes = RestRecord.finishOfList record.RestTimes }
            else
                // Restart work
                let! restarted = TimeDuration.createRestart record.Duration
                return { record with Duration = restarted }
        }
