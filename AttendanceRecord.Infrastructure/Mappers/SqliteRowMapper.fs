namespace AttendanceRecord.Infrastructure.Mappers

open System
open System.Globalization
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects
open AttendanceRecord.Persistence.Storage

module private DateTimeText =
   let fromDateTime (value: DateTime) : string =
      value.ToString("O", CultureInfo.InvariantCulture)

   let toDateTime (value: string) : DateTime =
      DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)

   let toWorkDate (value: DateTime) : string =
      value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)

module SqliteRowMapper =
   let toWorkRecordSqlRow (record: WorkRecord) : WorkRecordSqlRow =
      let row = WorkRecordSqlRow()
      row.Id <- record.Id.ToString()
      row.StartedAt <- record.Duration |> TimeDuration.getStartedAt |> DateTimeText.fromDateTime

      row.EndedAt <-
         record.Duration
         |> TimeDuration.getEndedAt
         |> Option.map DateTimeText.fromDateTime
         |> Option.toObj

      row.WorkDate <- record |> WorkRecord.getDate |> DateTimeText.toWorkDate
      row

   let toRestRecordSqlRows (workRecordId: Guid) (rests: RestRecord list) : RestRecordSqlRow list =
      rests
      |> RestRecordList.getSorted
      |> List.map (fun rest ->
         let row = RestRecordSqlRow()
         row.Id <- rest.Id.ToString()
         row.WorkRecordId <- workRecordId.ToString()
         row.StartedAt <- rest.Duration |> TimeDuration.getStartedAt |> DateTimeText.fromDateTime

         row.EndedAt <-
            rest.Duration
            |> TimeDuration.getEndedAt
            |> Option.map DateTimeText.fromDateTime
            |> Option.toObj

         row.Variant <-
            match rest.Variant with
            | RegularRest -> 0L
            | PaidRest -> 1L

         row)

   let toDomainRestRecord (row: RestRecordSqlRow) : RestRecord =
      let duration =
         TimeDuration.hydrate
            (row.StartedAt |> DateTimeText.toDateTime)
            (row.EndedAt |> Option.ofObj |> Option.map DateTimeText.toDateTime)

      let variant =
         match row.Variant with
         | 1L -> PaidRest
         | _ -> RegularRest

      RestRecord.hydrate (Guid.Parse row.Id) variant duration

   let toDomainWorkRecord
      (workRow: WorkRecordSqlRow)
      (restRows: RestRecordSqlRow list)
      : WorkRecord =
      let duration =
         TimeDuration.hydrate
            (workRow.StartedAt |> DateTimeText.toDateTime)
            (workRow.EndedAt |> Option.ofObj |> Option.map DateTimeText.toDateTime)

      let restRecords =
         restRows |> List.map toDomainRestRecord |> RestRecordList.getSorted

      WorkRecord.hydrate (Guid.Parse workRow.Id) duration restRecords
