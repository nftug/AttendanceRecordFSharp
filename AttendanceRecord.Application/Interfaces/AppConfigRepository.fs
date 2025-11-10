namespace AttendanceRecord.Application.Interfaces

open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities

type AppConfigRepository =
    { GetConfig: CancellationToken -> TaskResult<AppConfig, string>
      SaveConfig: AppConfig -> CancellationToken -> TaskResult<unit, string> }
