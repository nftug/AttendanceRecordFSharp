namespace AttendanceRecord.Domain.Entities

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.ValueObjects

type WorkTimeRecord =
    { Id: Guid
      Duration: TimeDuration
      RestTimes: RestTimeRecord list }

module WorkTimeRecord =
    let hydrate (id: Guid) (duration: TimeDuration) (restTimes: RestTimeRecord list) : WorkTimeRecord =
        { Id = id
          Duration = duration
          RestTimes = restTimes |> RestTimeRecord.getSortedList }

    // Status getters
    let getStartedAt (record: WorkTimeRecord) : DateTime =
        record.Duration |> TimeDuration.getStartedAt

    let getEndedAt (record: WorkTimeRecord) : DateTime option =
        record.Duration |> TimeDuration.getEndedAt

    let getRestDuration (record: WorkTimeRecord) : TimeSpan =
        record.RestTimes |> RestTimeRecord.getDurationOfList

    let getDuration (record: WorkTimeRecord) : TimeSpan =
        TimeDuration.getDuration record.Duration - getRestDuration record

    let getOvertimeDuration (standardWorkTime: TimeSpan) (record: WorkTimeRecord) : TimeSpan =
        getDuration record - standardWorkTime

    let hasDate (date: DateTime) (record: WorkTimeRecord) : bool = (getStartedAt record).Date = date.Date

    let isTodays (record: WorkTimeRecord) : bool = hasDate DateTime.Now.Date record

    let isActive (record: WorkTimeRecord) : bool = TimeDuration.isActive record.Duration

    let isResting (record: WorkTimeRecord) : bool =
        record.RestTimes |> RestTimeRecord.isRestingOfList

    let isWorking (record: WorkTimeRecord) : bool =
        isActive record && not (isResting record)

    // Factory methods
    let create (duration: TimeDuration) (restTimes: RestTimeRecord list) : WorkTimeRecord =
        { Id = Guid.NewGuid()
          Duration = duration
          RestTimes = restTimes |> RestTimeRecord.getSortedList }

    let update
        (newDuration: TimeDuration)
        (newRestTimes: RestTimeRecord list)
        (record: WorkTimeRecord)
        : WorkTimeRecord =
        { record with
            Duration = newDuration
            RestTimes = newRestTimes |> RestTimeRecord.getSortedList }

    let createStart () : WorkTimeRecord =
        { Id = Guid.NewGuid()
          Duration = TimeDuration.createStart ()
          RestTimes = [] }

    let toggleRest (record: WorkTimeRecord) : Result<WorkTimeRecord, string> =
        if not (isActive record) then
            Error "Work time is not active"
        else
            let restTimes = record.RestTimes |> RestTimeRecord.toggleOfList
            Ok { record with RestTimes = restTimes }

    let toggleWork (record: WorkTimeRecord) : Result<WorkTimeRecord, string> =
        result {
            if not (isTodays record) then
                return! Error "Can only toggle work for today's record"
            else if isActive record then
                // End work
                let! endedDuration = TimeDuration.createEnd record.Duration

                return
                    { record with
                        Duration = endedDuration
                        RestTimes = RestTimeRecord.finishOfList record.RestTimes }
            else
                // Restart work
                let! restarted = TimeDuration.createRestart record.Duration
                return { record with Duration = restarted }
        }
