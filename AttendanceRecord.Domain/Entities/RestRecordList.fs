namespace AttendanceRecord.Domain.Entities

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Errors

module RestRecordList =
   // --- Accessors ---
   let getSorted (records: RestRecord list) : RestRecord list =
      records |> List.sortBy RestRecord.getStartedAt

   let isResting (now: DateTime) (records: RestRecord list) : bool =
      records |> List.tryFind (RestRecord.isActive now) |> Option.isSome

   let getDuration (now: DateTime) (records: RestRecord list) : TimeSpan =
      records
      |> List.sumBy (RestRecord.getDuration now >> _.Ticks)
      |> TimeSpan.FromTicks

   let add (record: RestRecord) (records: RestRecord list) : RestRecord list =
      records
      |> List.filter (fun r -> r.Id <> record.Id)
      |> List.append [ record ]
      |> getSorted

   // --- State transitions ---
   let addStart (records: RestRecord list) : RestRecord list =
      records |> add (RestRecord.createStart ())

   let tryAddEnd
      (now: DateTime)
      (records: RestRecord list)
      : Result<RestRecord list, RestRecordError> =
      result {
         match records |> List.filter (RestRecord.isActive now) |> List.tryLast with
         | Some lastActive ->
            let! endedRest = lastActive |> RestRecord.tryCreateEnd now
            return records |> add endedRest
         | None -> return records
      }

   let tryAddToggled
      (now: DateTime)
      (records: RestRecord list)
      : Result<RestRecord list, RestRecordError> =
      match records |> isResting now with
      | true -> records |> tryAddEnd now
      | false -> Ok(addStart records)
