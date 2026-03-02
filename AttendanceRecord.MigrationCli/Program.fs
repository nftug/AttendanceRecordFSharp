open System
open System.IO
open System.Text.Json
open System.Threading
open System.Threading.Tasks
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Domain.ValueObjects
open AttendanceRecord.Persistence.Constants
open AttendanceRecord.Persistence.Storage
open AttendanceRecord.Infrastructure.Mappers
open AttendanceRecord.Infrastructure.Services

let private usage () =
   printfn "Usage: dotnet run --project AttendanceRecord.MigrationCli -- migrate [--app-dir <path>]"

let private parseArgs (argv: string array) : Result<string option, string> =
   if argv.Length = 0 then
      Error "コマンドが指定されていません。"
   elif argv[0] <> "migrate" then
      Error $"不明なコマンドです: {argv[0]}"
   else
      let rec loop index appDir =
         if index >= argv.Length then
            Ok appDir
         else
            match argv[index] with
            | "--app-dir" when index + 1 < argv.Length -> loop (index + 2) (Some argv[index + 1])
            | option when option.StartsWith("--app-dir=") ->
               let value = option.Substring("--app-dir=".Length)

               if String.IsNullOrWhiteSpace value then
                  Error "--app-dir の値が空です。"
               else
                  loop (index + 1) (Some value)
            | "--help"
            | "-h" -> Error "help"
            | unknown -> Error $"不明なオプションです: {unknown}"

      loop 1 None

let private getAppDirectoryService (appDir: string option) : AppDirectoryService =
   match appDir with
   | Some path ->
      Directory.CreateDirectory path |> ignore
      { Value = path }
   | None -> AppDirectoryService.createWithAppName "AttendanceRecord"

let private loadWorkRecordsJson
   (filePath: string)
   (ct: CancellationToken)
   : Task<Result<WorkRecord list, string>> =
   task {
      try
         use stream =
            new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)

         if stream.Length = 0L then
            return Ok []
         else
            let! dtos =
               JsonSerializer.DeserializeAsync(
                  stream,
                  InfraJsonContext.Intended.IEnumerableWorkRecordFileDto,
                  ct
               )

            let records =
               dtos
               |> Option.ofObj
               |> Option.defaultValue Seq.empty
               |> WorkRecordFileDtoMapper.toDomain

            return Ok records
      with ex ->
         return Error $"勤務記録JSONの読み込みに失敗しました: {ex.Message}"
   }

let private saveWorkRecords
   (store: WorkRecordMigrationSqliteStore)
   (workRecords: WorkRecord list)
   (ct: CancellationToken)
   : Task<Result<unit, string>> =
   task {
      try
         let workRows =
            workRecords |> List.map SqliteRowMapper.toWorkRecordSqlRow |> List.toArray

         let restRows =
            workRecords
            |> List.collect (fun workRecord ->
               SqliteRowMapper.toRestRecordSqlRows workRecord.Id workRecord.RestRecords)
            |> List.toArray

         do! store.SaveManyAsync(workRows, restRows, ct)

         return Ok()
      with ex ->
         return Error $"勤務記録の保存に失敗しました: {ex.Message}"
   }

let private migrate (appDirOption: string option) : Task<int> =
   task {
      let ct = CancellationToken.None

      try
         let appDirService = getAppDirectoryService appDirOption
         let workRecordsPath = Path.Combine(appDirService.Value, "workRecords.json")
         let dbPath = SqliteDatabase.getDbPath appDirService
         let store = WorkRecordMigrationSqliteStore dbPath
         let! hasExistingData = store.HasAnyDataAsync ct

         if hasExistingData then
            printfn "既存データ検出のため移行を中断しました。"
            return 1
         else
            let! workRecordsResult =
               task {
                  if File.Exists workRecordsPath then
                     let! loaded = loadWorkRecordsJson workRecordsPath ct
                     return loaded
                  else
                     return Ok []
               }

            match workRecordsResult with
            | Error error ->
               printfn "%s" error
               return 1
            | Ok workRecords ->
               let! saveResult = saveWorkRecords store workRecords ct

               match saveResult with
               | Error error ->
                  printfn "%s" error
                  return 1
               | Ok() ->
                  printfn "移行が完了しました。"
                  printfn "appDir: %s" appDirService.Value
                  printfn "db: %s" dbPath
                  printfn "workRecords: %d" workRecords.Length
                  return 0
      with ex ->
         printfn "予期しないエラー: %s" ex.Message
         return 2
   }

[<EntryPoint>]
let main argv =
   match parseArgs argv with
   | Ok appDirOption -> migrate appDirOption |> fun t -> t.GetAwaiter().GetResult()
   | Error "help" ->
      usage ()
      0
   | Error error ->
      printfn "%s" error
      usage ()
      1
