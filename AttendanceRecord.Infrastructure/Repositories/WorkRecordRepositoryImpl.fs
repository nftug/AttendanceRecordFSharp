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

    let private loadWorkRecords filePath ct =
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
                                InfraJsonContext.Intended.IEnumerableWorkRecordFileDto,
                                ct
                            )

                        match dtos |> Option.ofObj with
                        | None -> return []
                        | Some dtos -> return dtos |> WorkRecordFileDtoMapper.toDomain
            with ex ->
                return! Error $"Failed to load work records: {ex.Message}"
        }

    let private saveWorkRecords filePath workRecords ct =
        taskResult {
            try
                let workRecordDtos = workRecords |> WorkRecordFileDtoMapper.fromDomain

                use stream =
                    new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)

                do!
                    JsonSerializer.SerializeAsync(
                        stream,
                        workRecordDtos,
                        InfraJsonContext.Intended.IEnumerableWorkRecordFileDto,
                        ct
                    )

                do! stream.FlushAsync()
            with ex ->
                return! Error $"Failed to save work records: {ex.Message}"
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
