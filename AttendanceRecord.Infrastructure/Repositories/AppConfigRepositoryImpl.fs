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

    let private getConfig
        (appDirService: AppDirectoryService)
        (ct: CancellationToken)
        : TaskResult<AppConfig, string> =
        taskResult {
            let filePath = getFilePath appDirService

            try
                if not (File.Exists filePath) then
                    return AppConfig.initial
                else
                    use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)

                    if stream.Length = 0L then
                        return AppConfig.initial
                    else
                        let! dto =
                            JsonSerializer.DeserializeAsync(
                                stream,
                                InfraJsonContext.Intended.AppConfigFileDto,
                                ct
                            )

                        match dto |> Option.ofObj with
                        | None -> return AppConfig.initial
                        | Some dto -> return dto |> AppConfigFileDtoMapper.toDomain
            with ex ->
                return! Error $"Failed to load AppConfig: {ex.Message}"
        }

    let private saveConfig
        (appDirService: AppDirectoryService)
        (config: AppConfig)
        (ct: CancellationToken)
        : TaskResult<unit, string> =
        taskResult {
            let filePath = getFilePath appDirService

            try
                let dto = config |> AppConfigFileDtoMapper.fromDomain

                use stream =
                    new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)

                do!
                    JsonSerializer.SerializeAsync(
                        stream,
                        dto,
                        InfraJsonContext.Intended.AppConfigFileDto,
                        ct
                    )

                do! stream.FlushAsync()
            with ex ->
                return! Error $"Failed to save AppConfig: {ex.Message}"
        }

    let create (appDirService: AppDirectoryService) : AppConfigRepository =
        { GetConfig = getConfig appDirService
          SaveConfig = saveConfig appDirService }
