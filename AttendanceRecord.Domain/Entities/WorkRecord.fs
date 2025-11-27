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

    let getDate (record: WorkRecord) : DateTime = getStartedAt record |> _.Date

    let getRestDuration (now: DateTime) (variant: RestVariant) (record: WorkRecord) : TimeSpan =
        record.RestRecords
        |> List.filter (fun r -> r.Variant = variant)
        |> RestRecord.getDurationOfList now

    let getWorkDuration (now: DateTime) (record: WorkRecord) : TimeSpan =
        let baseDur = TimeDuration.getDuration now record.Duration

        let restDur = record |> getRestDuration now RegularRest

        baseDur - restDur

    let getWorkDurationWithPaidRest (now: DateTime) (record: WorkRecord) : TimeSpan =
        let baseDur = TimeDuration.getDuration now record.Duration
        let regularRestDur = record |> getRestDuration now RegularRest
        let paidRestDur = record |> getRestDuration now PaidRest

        baseDur - regularRestDur + paidRestDur

    let getOvertimeDuration
        (now: DateTime)
        (standardWorkTime: TimeSpan)
        (record: WorkRecord)
        : TimeSpan =
        getWorkDurationWithPaidRest now record - standardWorkTime

    let hasDate (date: DateTime) (record: WorkRecord) : bool = getDate record = date.Date

    let isActive (now: DateTime) (record: WorkRecord) : bool =
        record.Duration |> TimeDuration.isActive now

    let isResting (now: DateTime) (record: WorkRecord) : bool =
        record.RestRecords
        |> List.filter (fun r -> r.Variant = RegularRest)
        |> RestRecord.isRestingOfList now

    let isWorking (now: DateTime) (record: WorkRecord) : bool =
        isActive now record && not (isResting now record)

    // Factory methods
    let private checkDurationAndRests
        (duration: TimeDuration)
        (restTimes: RestRecord list)
        : Result<unit, string> =
        let date = duration |> TimeDuration.getDate

        result {
            for rest in restTimes do
                let workStartedAt = duration |> TimeDuration.getStartedAt

                let workEndedAt =
                    duration
                    |> TimeDuration.getEndedAt
                    |> Option.defaultValue (date.AddDays(1).AddSeconds(-1))

                let restStartedAt = rest |> RestRecord.getStartedAt

                let restEndedAt =
                    rest
                    |> RestRecord.getEndedAt
                    |> Option.defaultValue (date.AddDays(1).AddSeconds(-1))

                match rest.Variant with
                | RegularRest when restStartedAt < workStartedAt || restStartedAt.Date <> date ->
                    return!
                        Error $"Regular rest start time {restStartedAt} is outside of work duration"
                | RegularRest when restEndedAt.Date <> date ->
                    return! Error $"Regular rest end time {restEndedAt} is outside of work date"
                | RegularRest when restEndedAt > workEndedAt ->
                    return! Error $"Regular rest end time {restEndedAt} is outside of work duration"
                | PaidRest when (RestRecord.getEndedAt rest).IsNone ->
                    return! Error $"Paid rest must have an end time"
                | PaidRest when restStartedAt.Date <> date ->
                    return! Error $"Paid rest start time {restStartedAt} is outside of work date"
                | PaidRest when restEndedAt > workStartedAt && restStartedAt < workEndedAt ->
                    return!
                        Error
                            $"Paid rest time {restStartedAt} - {restEndedAt} overlaps with work duration"
                | _ -> ()
        }

    let tryCreate
        (duration: TimeDuration)
        (restTimes: RestRecord list)
        : Result<WorkRecord, string> =
        checkDurationAndRests duration restTimes
        |> Result.map (fun () ->
            { Id = Guid.NewGuid()
              Duration = duration
              RestRecords = restTimes |> RestRecord.getSortedList })

    let tryUpdate
        (newDuration: TimeDuration)
        (newRestTimes: RestRecord list)
        (record: WorkRecord)
        : Result<WorkRecord, string> =
        checkDurationAndRests newDuration newRestTimes
        |> Result.map (fun () ->
            { record with
                Duration = newDuration
                RestRecords = newRestTimes |> RestRecord.getSortedList })

    let createStart () : WorkRecord =
        { Id = Guid.NewGuid()
          Duration = TimeDuration.createStart ()
          RestRecords = [] }

    let tryToggleRest (now: DateTime) (record: WorkRecord) : Result<WorkRecord, string> =
        result {
            if not (record |> isActive now) then
                return! Error "Can only toggle rest for active work record"
            else
                let! restRecords = record.RestRecords |> RestRecord.toggleOfList now

                return
                    { record with
                        RestRecords = restRecords }
        }

    let tryToggleWork (now: DateTime) (record: WorkRecord) : Result<WorkRecord, string> =
        result {
            if not (record |> hasDate now) then
                return! Error "Can only toggle work for today's record"
            else
                match isActive now record, getEndedAt record with
                | true, _ ->
                    // End work
                    let! endedDuration = TimeDuration.tryCreateEnd record.Duration

                    let! restRecords =
                        record.RestRecords
                        |> List.filter (fun r -> r.Variant = RegularRest)
                        |> RestRecord.finishOfList now

                    return
                        { record with
                            Duration = endedDuration
                            RestRecords = restRecords }
                | false, Some endedAt ->
                    // Restart work
                    let! restarted = TimeDuration.tryCreateRestart record.Duration

                    let! restRecords =
                        TimeDuration.tryCreate endedAt (Some now)
                        |> Result.map (RestRecord.create (Guid.NewGuid()) RegularRest)
                        |> Result.map (fun rr -> record.RestRecords |> RestRecord.addToList rr)

                    return
                        { record with
                            Duration = restarted
                            RestRecords = restRecords }
                | false, None -> return! Error "Invalid work record state to toggle work"
        }

    // List operations
    let getSortedList (records: WorkRecord list) : WorkRecord list =
        records |> List.sortBy getStartedAt
