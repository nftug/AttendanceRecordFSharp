namespace AttendanceRecord.Infrastructure.Repositories

open System
open System.IO
open System.Text.Json
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open AttendanceRecord.Persistence.Constants
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Infrastructure.Services
open AttendanceRecord.Infrastructure.Mappers

module WorkRecordRepositoryImpl =
   let private getFilePath appDir =
      Path.Combine(appDir.Value, "workRecords.json")

   let private getBackupPath filePath = $"{filePath}.bak"

   let private getTempPath (filePath: string) =
      let dir = Path.GetDirectoryName filePath
      let fileName = Path.GetFileName filePath
      let tempName = $"{fileName}.{Guid.NewGuid()}.tmp"
      Path.Combine(dir, tempName)

   let private deleteIfExists filePath =
      try
         if File.Exists filePath then
            File.Delete filePath
      with _ ->
         ()

   let private loadWorkRecordsCore filePath ct =
      task {
         try
            use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)

            if stream.Length = 0L then
               return Error "ファイルが空です。"
            else
               let! dtos =
                  JsonSerializer.DeserializeAsync(
                     stream,
                     InfraJsonContext.Intended.IEnumerableWorkRecordFileDto,
                     ct
                  )

               match dtos |> Option.ofObj with
               | None -> return Error "勤務記録の内容が空です。"
               | Some dtos -> return Ok(dtos |> WorkRecordFileDtoMapper.toDomain)
         with ex ->
            return Error ex.Message
      }

   let private loadWorkRecords filePath ct =
      task {
         let backupPath = getBackupPath filePath

         if not (File.Exists filePath) then
            if File.Exists backupPath then
               let! backupResult = loadWorkRecordsCore backupPath ct

               return
                  match backupResult with
                  | Ok records -> Ok records
                  | Error backupError -> Error $"勤務記録の読み込みに失敗しました: {backupError}"
            else
               return Ok []
         else
            let! primaryResult = loadWorkRecordsCore filePath ct

            match primaryResult with
            | Ok records -> return Ok records
            | Error primaryError when File.Exists backupPath ->
               let! backupResult = loadWorkRecordsCore backupPath ct

               return
                  match backupResult with
                  | Ok records -> Ok records
                  | Error backupError ->
                     Error $"勤務記録の読み込みに失敗しました: {primaryError} (バックアップ: {backupError})"
            | Error primaryError -> return Error $"勤務記録の読み込みに失敗しました: {primaryError}"
      }

   let private saveWorkRecords filePath workRecords ct =
      taskResult {
         let tempPath = getTempPath filePath

         try
            let workRecordDtos = workRecords |> WorkRecordFileDtoMapper.fromDomain

            use stream =
               new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None)

            do!
               JsonSerializer.SerializeAsync(
                  stream,
                  workRecordDtos,
                  InfraJsonContext.Intended.IEnumerableWorkRecordFileDto,
                  ct
               )

            do! stream.FlushAsync()

            if File.Exists filePath then
               File.Copy(filePath, getBackupPath filePath, true)
               File.Move(tempPath, filePath, true)
            else
               File.Move(tempPath, filePath)
         with ex ->
            deleteIfExists tempPath
            return! Error $"勤務記録の保存に失敗しました: {ex.Message}"
      }

   let private getByDate appDir date ct =
      taskResult {
         let! workRecords = loadWorkRecords (getFilePath appDir) ct
         return workRecords |> List.tryFind (WorkRecord.hasDate date)
      }

   let private getById appDir id ct =
      taskResult {
         let! workRecords = loadWorkRecords (getFilePath appDir) ct
         return workRecords |> List.tryFind (fun record -> record.Id = id)
      }

   let private getByMonth appDir (monthDate: DateTime) ct =
      taskResult {
         let! workRecords = loadWorkRecords (getFilePath appDir) ct

         return
            workRecords
            |> List.filter (fun record ->
               let startedAt = WorkRecord.getDate record
               startedAt.Year = monthDate.Year && startedAt.Month = monthDate.Month)
      }

   let private saveWorkRecord filePath (workRecord: WorkRecord) ct =
      taskResult {
         let! existingRecords = loadWorkRecords filePath ct

         do!
            existingRecords
            |> List.filter (fun wr -> wr.Id <> workRecord.Id)
            |> List.append [ workRecord ]
            |> fun records -> saveWorkRecords filePath records ct
      }

   let private deleteWorkRecord filePath id ct =
      taskResult {
         let! existingRecords = loadWorkRecords filePath ct

         do!
            existingRecords
            |> List.filter (fun wr -> wr.Id <> id)
            |> fun records -> saveWorkRecords filePath records ct
      }

   let create (appDir: AppDirectoryService) : WorkRecordRepository =
      { Save = saveWorkRecord (getFilePath appDir)
        Delete = deleteWorkRecord (getFilePath appDir)
        GetByDate = getByDate appDir
        GetById = getById appDir
        GetMonthly = getByMonth appDir }
