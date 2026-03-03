namespace AttendanceRecord.Infrastructure.Repositories

open System
open System.Globalization
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Infrastructure.Mappers
open AttendanceRecord.Infrastructure.Services
open AttendanceRecord.Persistence.Storage

module WorkRecordRepositoryImpl =
   let private toWorkDateText (date: DateTime) : string =
      date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)

   let private toDomainRecord (aggregate: WorkRecordAggregateSqlRow) : WorkRecord =
      SqliteRowMapper.toDomainWorkRecord aggregate.WorkRecord (aggregate.RestRecords |> Seq.toList)

   let private saveWorkRecord
      (store: WorkRecordSqliteStore)
      (workRecord: WorkRecord)
      (ct: CancellationToken)
      =
      taskResult {
         try
            let workRow = workRecord |> SqliteRowMapper.toWorkRecordSqlRow

            let restRows =
               workRecord.RestRecords
               |> SqliteRowMapper.toRestRecordSqlRows workRecord.Id
               |> List.toArray

            do! store.SaveAsync(workRow, restRows, ct)
            return ()
         with ex ->
            return! Error $"勤務記録の保存に失敗しました: {ex.Message}"
      }

   let private deleteWorkRecord (store: WorkRecordSqliteStore) (id: Guid) (ct: CancellationToken) =
      taskResult {
         try
            do! store.DeleteAsync(id.ToString(), ct)
            return ()
         with ex ->
            return! Error $"勤務記録の削除に失敗しました: {ex.Message}"
      }

   let private getById (store: WorkRecordSqliteStore) (id: Guid) (ct: CancellationToken) =
      taskResult {
         try
            let! aggregateOpt = store.GetByIdAsync(id.ToString(), ct)
            return aggregateOpt |> Option.ofObj |> Option.map toDomainRecord
         with ex ->
            return! Error $"勤務記録の取得に失敗しました: {ex.Message}"
      }

   let private getByDate (store: WorkRecordSqliteStore) (date: DateTime) (ct: CancellationToken) =
      taskResult {
         try
            let! aggregateOpt = store.GetByDateAsync(toWorkDateText date, ct)
            return aggregateOpt |> Option.ofObj |> Option.map toDomainRecord
         with ex ->
            return! Error $"勤務記録の取得に失敗しました: {ex.Message}"
      }

   let private getByMonth
      (store: WorkRecordSqliteStore)
      (monthDate: DateTime)
      (ct: CancellationToken)
      =
      taskResult {
         try
            let monthStart = DateTime(monthDate.Year, monthDate.Month, 1)
            let monthEnd = monthStart.AddMonths 1

            let! aggregates =
               store.GetByMonthAsync(toWorkDateText monthStart, toWorkDateText monthEnd, ct)

            return aggregates |> Seq.map toDomainRecord |> Seq.toList |> WorkRecordList.getSorted
         with ex ->
            return! Error $"勤務記録の月次取得に失敗しました: {ex.Message}"
      }

   let create (appDir: AppDirectoryService) : WorkRecordRepository =
      let store = SqliteDatabase.getDbStore appDir

      { Save = saveWorkRecord store
        Delete = deleteWorkRecord store
        GetByDate = getByDate store
        GetById = getById store
        GetMonthly = getByMonth store }
