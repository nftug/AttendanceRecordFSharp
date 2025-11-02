namespace AttendanceRecord.Infrastructure.Services

open System
open System.Reflection
open System.IO

type AppDirectoryService = { Value: string }

module AppDirectoryService =
    let create () : AppDirectoryService =
        let assembly = Assembly.GetEntryAssembly()

        if assembly = null then
            failwith "Unable to determine the entry assembly."

        let appName =
            assembly.GetName().Name
            |> Option.ofObj
            |> Option.defaultValue "AttendanceRecord"

        let basePath = Environment.GetFolderPath Environment.SpecialFolder.ApplicationData
        let appDataPath = Path.Combine(basePath, appName)
        Directory.CreateDirectory appDataPath |> ignore

        { Value = appDataPath }
