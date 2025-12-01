namespace AttendanceRecord.Domain.Helpers

open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.ValueObjects
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.Errors

module WorkRecordHelper =
    let validateRestRecords
        (workDuration: TimeDuration)
        (restRecords: RestRecord list)
        : Validation<unit, WorkRecordError> =
        let date = workDuration |> TimeDuration.getDate
        let workStartedAt = workDuration |> TimeDuration.getStartedAt
        let workEndedAtOpt = workDuration |> TimeDuration.getEndedAt

        let workEndedAtFilled =
            workEndedAtOpt |> Option.defaultValue (date.AddDays(1).AddSeconds(-1))

        let validateNoOverlap () =
            validation {
                let overlapErrorMessage = "他の休憩時間と重複しています。"

                let overlaps ((r1, r2): RestRecord * RestRecord) : bool =
                    let s1 = r1 |> RestRecord.getStartedAt
                    let e1 = r1 |> RestRecord.getEndedAt |> Option.defaultValue workEndedAtFilled
                    let s2 = r2 |> RestRecord.getStartedAt
                    let e2 = r2 |> RestRecord.getEndedAt |> Option.defaultValue workEndedAtFilled
                    s1 < e2 && s2 < e1

                let overlappingPairs = restRecords |> List.pairwise |> List.filter overlaps

                if not overlappingPairs.IsEmpty then
                    let errors =
                        overlappingPairs
                        |> List.collect (fun (r1, r2) ->
                            [ RestDurationError(r1.Id, TimeDurationError overlapErrorMessage)
                              RestDurationError(r2.Id, TimeDurationError overlapErrorMessage) ])

                    return! Error(WorkRestsErrors errors)
            }

        let validateRestData (rest: RestRecord) =
            validation {
                let restStartedAt = rest |> RestRecord.getStartedAt
                let restEndedAtOpt = rest |> RestRecord.getEndedAt

                let wrapError (error: string) =
                    Error(RestDurationError(rest.Id, TimeDurationError error))

                match rest.Variant with
                | RegularRest ->
                    let restEndedAt =
                        restEndedAtOpt |> Option.defaultValue (date.AddDays(1).AddSeconds(-1))

                    if restStartedAt < workStartedAt || restEndedAt > workEndedAtFilled then
                        return! wrapError "通常休憩の期間が勤務時間外です。"
                | PaidRest ->
                    match restEndedAtOpt with
                    | None -> return! wrapError "有給休暇には終了時刻が必要です。"
                    | Some restEndedAt ->
                        if restStartedAt.Date <> date then
                            return! wrapError "有給休暇の開始時刻が勤務日と異なります。"
                        else if
                            restStartedAt < workEndedAtFilled && restEndedAt > workStartedAt
                        then
                            return! wrapError "有給休暇の時間帯が勤務時間と重複しています。"
            }

        validation {
            let noOverlapResult = validateNoOverlap ()

            let restDatesResult =
                restRecords
                |> List.map (validateRestData >> Result.mapError WorkRestsErrors)
                |> List.sequenceResultA
                |> Result.map (fun _ -> ())

            do! Validation.zip noOverlapResult restDatesResult |> Validation.map (fun _ -> ())
        }
