namespace AttendanceRecord.Infrastructure.Repositories

open System.IO
open System.Text.Json
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Persistence.Constants
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Infrastructure.Services
open AttendanceRecord.Infrastructure.Mappers

module AppConfigRepositoryImpl =
   let private getFilePath appDir =
      Path.Combine(appDir.Value, "appConfig.json")

   let private getBackupPath filePath = $"{filePath}.bak"

   let private getTempPath (filePath: string) =
      let dir = Path.GetDirectoryName filePath
      let fileName = Path.GetFileName filePath
      let tempName = $"{fileName}.{System.Guid.NewGuid()}.tmp"
      Path.Combine(dir, tempName)

   let private deleteIfExists filePath =
      try
         if File.Exists filePath then
            File.Delete filePath
      with _ ->
         ()

   let private loadConfigCore filePath ct =
      task {
         try
            use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)

            if stream.Length = 0L then
               return Ok AppConfig.initial
            else
               let! dto =
                  JsonSerializer.DeserializeAsync(
                     stream,
                     InfraJsonContext.Intended.AppConfigFileDto,
                     ct
                  )

               match dto |> Option.ofObj with
               | None -> return Ok AppConfig.initial
               | Some dto -> return Ok(dto |> AppConfigFileDtoMapper.toDomain)
         with ex ->
            return Error ex.Message
      }

   let private getConfig
      (appDirService: AppDirectoryService)
      (ct: CancellationToken)
      : TaskResult<AppConfig, string> =
      task {
         let filePath = getFilePath appDirService
         let backupPath = getBackupPath filePath

         if not (File.Exists filePath) then
            if File.Exists backupPath then
               let! backupResult = loadConfigCore backupPath ct

               return
                  match backupResult with
                  | Ok config -> Ok config
                  | Error backupError -> Error $"アプリ設定の読み込みに失敗しました: {backupError}"
            else
               return Ok AppConfig.initial
         else
            let! primaryResult = loadConfigCore filePath ct

            match primaryResult with
            | Ok config -> return Ok config
            | Error primaryError when File.Exists backupPath ->
               let! backupResult = loadConfigCore backupPath ct

               return
                  match backupResult with
                  | Ok config -> Ok config
                  | Error backupError ->
                     Error $"アプリ設定の読み込みに失敗しました: {primaryError} (バックアップ: {backupError})"
            | Error primaryError -> return Error $"アプリ設定の読み込みに失敗しました: {primaryError}"
      }

   let private saveConfig
      (appDirService: AppDirectoryService)
      (config: AppConfig)
      (ct: CancellationToken)
      : TaskResult<unit, string> =
      taskResult {
         let filePath = getFilePath appDirService
         let tempPath = getTempPath filePath

         try
            let dto = config |> AppConfigFileDtoMapper.fromDomain

            use stream =
               new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None)

            do!
               JsonSerializer.SerializeAsync(
                  stream,
                  dto,
                  InfraJsonContext.Intended.AppConfigFileDto,
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
            return! Error $"アプリ設定の保存に失敗しました: {ex.Message}"
      }

   let create (appDirService: AppDirectoryService) : AppConfigRepository =
      { GetConfig = getConfig appDirService
        SaveConfig = saveConfig appDirService }
