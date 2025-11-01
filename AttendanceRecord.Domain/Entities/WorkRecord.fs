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

    let getRestDuration (record: WorkRecord) : TimeSpan =
        record.RestRecords |> RestRecord.getDurationOfList

    let getDuration (record: WorkRecord) : TimeSpan =
        TimeDuration.getDuration record.Duration - getRestDuration record

    let getOvertimeDuration (standardWorkTime: TimeSpan) (record: WorkRecord) : TimeSpan =
        getDuration record - standardWorkTime

    let hasDate (date: DateTime) (record: WorkRecord) : bool = getDate record = date.Date

    let isActive (record: WorkRecord) : bool = TimeDuration.isActive record.Duration

    let isResting (record: WorkRecord) : bool =
        record.RestRecords |> RestRecord.isRestingOfList

    let isWorking (record: WorkRecord) : bool =
        isActive record && not (isResting record)

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

    let toggleRest (record: WorkRecord) : Result<WorkRecord, string> =
        result {
            if not (isActive record) then
                return! Error "Can only toggle rest for active work record"
            else
                let! restRecords = record.RestRecords |> RestRecord.toggleOfList

                return
                    { record with
                        RestRecords = restRecords }
        }

    let toggleWork (record: WorkRecord) : Result<WorkRecord, string> =
        result {
            if not (record |> hasDate DateTime.Now) then
                return! Error "Can only toggle work for today's record"
            else
                match isActive record, getEndedAt record with
                | true, _ ->
                    // End work
                    let! endedDuration = TimeDuration.createEnd record.Duration
                    let! restRecords = RestRecord.finishOfList record.RestRecords

                    return
                        { record with
                            Duration = endedDuration
                            RestRecords = restRecords }
                | false, Some endedAt ->
                    // Restart work
                    let! restarted = TimeDuration.createRestart record.Duration
                    let! newRestRecord = TimeDuration.create endedAt (Some DateTime.Now) |> Result.map RestRecord.create
                    let restTimes = record.RestRecords |> RestRecord.addToList newRestRecord

                    return
                        { record with
                            Duration = restarted
                            RestRecords = restTimes }
                | false, None -> return! Error "Invalid work record state to toggle work"
        }

    // List operations
    let getSortedList (records: WorkRecord list) : WorkRecord list = records |> List.sortBy getStartedAt
