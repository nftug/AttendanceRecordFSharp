namespace AttendanceRecord.Infrastructure.Services

open System.IO
open AttendanceRecord.Persistence.Storage

module SqliteDatabase =
   [<Literal>]
   let DatabaseFileName = "attendance.db"

   let getDbPath (appDir: AppDirectoryService) : string =
      Path.Combine(appDir.Value, DatabaseFileName)

   let getDbStore (appDir: AppDirectoryService) : WorkRecordSqliteStore =
      let dbPath = getDbPath appDir
      new WorkRecordSqliteStore(dbPath)
