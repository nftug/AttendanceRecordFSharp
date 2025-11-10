namespace AttendanceRecord.Infrastructure.Services

open System.IO
open AttendanceRecord.Application.Interfaces

module SingleInstanceGuardImpl =
    let create (appDirectoryService: AppDirectoryService) : SingleInstanceGuard =
        let lockFilePath = Path.Combine(appDirectoryService.Value, "app.lock")
        let mutable fileStream: FileStream option = None

        let tryAcquireLock () : bool =
            try
                let stream =
                    new FileStream(
                        lockFilePath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None
                    )

                fileStream <- Some stream
                true
            with :? IOException ->
                // Lock file is already held by another instance
                false

        let releaseLock () : unit =
            match fileStream with
            | Some stream ->
                stream.Close()
                fileStream <- None

                if File.Exists(lockFilePath) then
                    File.Delete(lockFilePath)
            | None -> ()

        { TryAcquireLock = tryAcquireLock
          ReleaseLock = releaseLock }
