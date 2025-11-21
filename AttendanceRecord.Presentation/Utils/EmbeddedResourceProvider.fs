namespace AttendanceRecord.Presentation.Utils

open System
open System.Reflection

module EmbeddedResourceProvider =
    let private detectResourceNamespace (assembly: Assembly) (rootPath: string) =
        let segment = rootPath.TrimStart('/').Replace('/', '.')

        let matched =
            assembly.GetManifestResourceNames()
            |> Array.tryFind _.Contains(segment, StringComparison.OrdinalIgnoreCase)

        match matched with
        | Some name ->
            let index = name.IndexOf(segment, StringComparison.OrdinalIgnoreCase)
            name.Substring(0, index).TrimEnd('.')
        | None -> invalidOp "Resource not found."

    let getFileStream (filePath: string) =
        let assembly = Assembly.GetExecutingAssembly()
        let filePath = filePath.Replace('\\', '/')
        let rootPath = filePath.Split '/' |> Array.head

        let resourceNamespace = detectResourceNamespace assembly rootPath
        let fullResourceName = resourceNamespace + "." + filePath.Replace('/', '.')

        match assembly.GetManifestResourceStream fullResourceName |> Option.ofObj with
        | Some s -> s
        | None -> invalidOp $"Embedded resource '{fullResourceName}' not found."
