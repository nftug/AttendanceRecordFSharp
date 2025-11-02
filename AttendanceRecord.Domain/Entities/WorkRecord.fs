namespace AttendanceRecord.Domain.Entities

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.ValueObjects

type WorkRecord =
    { Id: Guid
      Duration: TimeDuration
      RestRecords: RestRecord list }

module WorkRecord =
    let hydrate (id: Guid) (duration: TimeDuration) (restTimes: RestRecord list) : WorkRecord =
        { Id = id
          Duration = duration
          RestRecords = restTimes |> RestRecord.getSortedList }

    // Status getters
    let getStartedAt (record: WorkRecord) : DateTime =
        record.Duration |> TimeDuration.getStartedAt

    let getEndedAt (record: WorkRecord) : DateTime option =
        record.Duration |> TimeDuration.getEndedAt

    let getDate (record: WorkRecord) : DateTime =
        getStartedAt record |> fun dt -> dt.Date

    let getRestDuration (now: DateTime) (record: WorkRecord) : TimeSpan =
        record.RestRecords |> RestRecord.getDurationOfList now

    let getDuration (now: DateTime) (record: WorkRecord) : TimeSpan =
        let baseDur = TimeDuration.getDuration now record.Duration
        let restDur = RestRecord.getDurationOfList now record.RestRecords
        baseDur - restDur

    let getOvertimeDuration (now: DateTime) (standardWorkTime: TimeSpan) (record: WorkRecord) : TimeSpan =
        getDuration now record - standardWorkTime

    let hasDate (date: DateTime) (record: WorkRecord) : bool = getDate record = date.Date

    let isActive (now: DateTime) (record: WorkRecord) : bool =
        record.Duration |> TimeDuration.isActive now

    let isResting (now: DateTime) (record: WorkRecord) : bool =
        record.RestRecords |> RestRecord.isRestingOfList now

    let isWorking (now: DateTime) (record: WorkRecord) : bool =
        isActive now record && not (isResting now record)

    // Factory methods
    let create (duration: TimeDuration) (restTimes: RestRecord list) : WorkRecord =
        { Id = Guid.NewGuid()
          Duration = duration
          RestRecords = restTimes |> RestRecord.getSortedList }

    let update (newDuration: TimeDuration) (newRestTimes: RestRecord list) (record: WorkRecord) : WorkRecord =
        { record with
            Duration = newDuration
            RestRecords = newRestTimes |> RestRecord.getSortedList }

    let createStart () : WorkRecord =
        { Id = Guid.NewGuid()
          Duration = TimeDuration.createStart ()
          RestRecords = [] }

    let toggleRest (now: DateTime) (record: WorkRecord) : Result<WorkRecord, string> =
        result {
            if not (isActive now record) then
                return! Error "Can only toggle rest for active work record"
            else
                let! restRecords = record.RestRecords |> RestRecord.toggleOfList now

                return
                    { record with
                        RestRecords = restRecords }
        }

    let toggleWork (now: DateTime) (record: WorkRecord) : Result<WorkRecord, string> =
        result {
            if not (record |> hasDate now) then
                return! Error "Can only toggle work for today's record"
            else
                match isActive now record, getEndedAt record with
                | true, _ ->
                    // End work
                    let! endedDuration = TimeDuration.createEnd record.Duration
                    let! restRecords = record.RestRecords |> RestRecord.finishOfList now

                    return
                        { record with
                            Duration = endedDuration
                            RestRecords = restRecords }
                | false, Some endedAt ->
                    // Restart work
                    let! restarted = TimeDuration.createRestart record.Duration

                    let! restRecords =
                        TimeDuration.create endedAt (Some now)
                        |> Result.map RestRecord.create
                        |> Result.map (fun rr -> record.RestRecords |> RestRecord.addToList rr)

                    return
                        { record with
                            Duration = restarted
                            RestRecords = restRecords }
                | false, None -> return! Error "Invalid work record state to toggle work"
        }

    // List operations
    let getSortedList (records: WorkRecord list) : WorkRecord list = records |> List.sortBy getStartedAt
