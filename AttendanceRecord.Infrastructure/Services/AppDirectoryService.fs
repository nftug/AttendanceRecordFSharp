namespace AttendanceRecord.Infrastructure.Services

open System
open System.Reflection
open System.IO

type AppDirectoryService = { Value: string }

module AppDirectoryService =
   let createWithAppName (appName: string) : AppDirectoryService =
      let basePath = Environment.GetFolderPath Environment.SpecialFolder.ApplicationData
      let appDataPath = Path.Combine(basePath, appName)
      Directory.CreateDirectory appDataPath |> ignore

      { Value = appDataPath }

   let create () : AppDirectoryService =
      let assembly = Assembly.GetEntryAssembly()

      if assembly = null then
         createWithAppName "AttendanceRecord"
      else
         let appName =
            assembly.GetName().Name
            |> Option.ofObj
            |> Option.defaultValue "AttendanceRecord"

         createWithAppName appName
