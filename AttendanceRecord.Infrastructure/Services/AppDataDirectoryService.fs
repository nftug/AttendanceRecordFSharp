namespace AttendanceRecord.Infrastructure.Services

open System
open System.Reflection
open System.IO

type AppDataDirectoryService =
    { AppDirectoryPath: string
      GetFilePath: string -> string }

module AppDataDirectoryService =
    let create () : AppDataDirectoryService =
        let assembly = Assembly.GetEntryAssembly()

        if assembly = null then
            failwith "Unable to get entry assembly."

        let appName =
            assembly.GetName().Name
            |> Option.ofObj
            |> Option.defaultValue "AttendanceRecord"

        let basePath = Environment.GetFolderPath Environment.SpecialFolder.ApplicationData
        let appDataPath = Path.Combine(basePath, appName)
        Directory.CreateDirectory appDataPath |> ignore

        { AppDirectoryPath = appDataPath
          GetFilePath = fun fileName -> Path.Combine(appDataPath, fileName) }
