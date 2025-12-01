namespace AttendanceRecord.Domain.Entities

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.ValueObjects
open AttendanceRecord.Domain.Errors
open AttendanceRecord.Domain.Helpers

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
    let tryCreate
        (duration: TimeDuration)
        (restTimes: RestRecord list)
        : Validation<WorkRecord, WorkRecordError> =
        WorkRecordHelper.validateRestRecords duration restTimes
        |> Result.map (fun _ ->
            { Id = Guid.NewGuid()
              Duration = duration
              RestRecords = restTimes |> RestRecord.getSortedList })

    let tryUpdate
        (newDuration: TimeDuration)
        (newRestTimes: RestRecord list)
        (record: WorkRecord)
        : Validation<WorkRecord, WorkRecordError> =
        WorkRecordHelper.validateRestRecords newDuration newRestTimes
        |> Result.map (fun _ ->
            { record with
                Duration = newDuration
                RestRecords = newRestTimes |> RestRecord.getSortedList })

    let createStart () : WorkRecord =
        { Id = Guid.NewGuid()
          Duration = TimeDuration.createStart ()
          RestRecords = [] }

    let tryToggleRest
        (now: DateTime)
        (record: WorkRecord)
        : Validation<WorkRecord, WorkRecordError> =
        validation {
            if not (record |> isActive now) then
                return! Error(WorkGenericError "終了済みの勤務記録では休憩の開始/終了を切り替えできません。")
            else
                let! restRecords =
                    record.RestRecords
                    |> RestRecord.toggleOfList now
                    |> Result.mapError (fun e -> WorkRestsErrors [ e ])

                return
                    { record with
                        RestRecords = restRecords }
        }

    let tryToggleWork
        (now: DateTime)
        (record: WorkRecord)
        : Validation<WorkRecord, WorkRecordError> =
        validation {
            if not (record |> hasDate now) then
                return! Error(WorkGenericError "開始/終了を切り替えられるのは今日の勤務のみです。")
            else
                match isActive now record, getEndedAt record with
                | true, _ ->
                    // End work
                    let! endedDuration =
                        TimeDuration.tryCreateEnd record.Duration
                        |> Result.mapError WorkDurationError

                    let! restRecords =
                        record.RestRecords
                        |> List.filter (fun r -> r.Variant = RegularRest)
                        |> RestRecord.finishOfList now
                        |> Result.mapError (fun e -> WorkRestsErrors [ e ])

                    return
                        { record with
                            Duration = endedDuration
                            RestRecords = restRecords }
                | false, Some endedAt ->
                    // Restart work
                    let! restarted =
                        TimeDuration.tryCreateRestart record.Duration
                        |> Result.mapError WorkDurationError

                    let! restRecords =
                        TimeDuration.tryCreate endedAt (Some now)
                        |> Result.map (RestRecord.create (Guid.NewGuid()) RegularRest)
                        |> Result.map (fun rr -> record.RestRecords |> RestRecord.addToList rr)
                        |> Result.mapError WorkDurationError

                    return
                        { record with
                            Duration = restarted
                            RestRecords = restRecords }
                | false, None -> return! Error(WorkGenericError "勤務記録の状態が不正です。")
        }

    // List operations
    let getSortedList (records: WorkRecord list) : WorkRecord list =
        records |> List.sortBy getStartedAt
