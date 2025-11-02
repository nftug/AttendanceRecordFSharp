namespace AttendanceRecord.Infrastructure.Repositories

open System
open System.IO
open System.Text.Json
open FsToolkit.ErrorHandling
open AttendanceRecord.Persistence.Constants
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Infrastructure.Services
open AttendanceRecord.Infrastructure.Mappers

module WorkRecordRepositoryImpl =
    let private getFilePath appDir =
        Path.Combine(appDir.Value, "workRecords.json")

    let private loadWorkRecords filePath =
        taskResult {
            try
                if not (File.Exists filePath) then
                    return []
                else
                    use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)

                    if stream.Length = 0L then
                        return []
                    else
                        let! dtos =
                            JsonSerializer.DeserializeAsync(
                                stream,
                                InfraJsonContext.Intended.IEnumerableWorkRecordFileDto
                            )

                        match dtos with
                        | dtos when dtos = null -> return []
                        | _ -> return dtos |> WorkRecordFileDtoMapper.toDomain
            with ex ->
                return! Error $"Failed to load work records: {ex.Message}"
        }

    let private saveWorkRecords filePath workRecords =
        taskResult {
            try
                let workRecordDtos = workRecords |> WorkRecordFileDtoMapper.fromDomain

                use stream =
                    new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)

                do!
                    JsonSerializer.SerializeAsync(
                        stream,
                        workRecordDtos,
                        InfraJsonContext.Intended.IEnumerableWorkRecordFileDto
                    )

                do! stream.FlushAsync()
            with ex ->
                return! Error $"Failed to save work records: {ex.Message}"
        }

    let private getByDate appDir date =
        taskResult {
            let! workRecords = loadWorkRecords (getFilePath appDir)
            return workRecords |> List.tryFind (WorkRecord.hasDate date)
        }

    let private getById appDir id =
        taskResult {
            let! workRecords = loadWorkRecords (getFilePath appDir)
            return workRecords |> List.tryFind (fun record -> record.Id = id)
        }

    let private getByMonth appDir (monthDate: DateTime) =
        taskResult {
            let! workRecords = loadWorkRecords (getFilePath appDir)

            return
                workRecords
                |> List.filter (fun record ->
                    let startedAt = WorkRecord.getDate record
                    startedAt.Year = monthDate.Year && startedAt.Month = monthDate.Month)
        }

    let private saveWorkRecord filePath (workRecord: WorkRecord) =
        taskResult {
            let! existingRecords = loadWorkRecords filePath

            do!
                existingRecords
                |> List.filter (fun wr -> wr.Id <> workRecord.Id)
                |> List.append [ workRecord ]
                |> saveWorkRecords filePath
        }

    let private deleteWorkRecord filePath id =
        taskResult {
            let! existingRecords = loadWorkRecords filePath

            do!
                existingRecords
                |> List.filter (fun wr -> wr.Id <> id)
                |> saveWorkRecords filePath
        }

    let create (appDir: AppDirectoryService) : WorkRecordRepository =
        { Save = saveWorkRecord (getFilePath appDir)
          Delete = deleteWorkRecord (getFilePath appDir)
          GetByDate = getByDate appDir
          GetById = getById appDir
          GetMonthly = getByMonth appDir }
