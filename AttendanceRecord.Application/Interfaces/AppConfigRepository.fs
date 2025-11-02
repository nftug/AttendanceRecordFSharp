namespace AttendanceRecord.Application.Interfaces

open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities

type AppConfigRepository =
    { GetConfig: unit -> TaskResult<AppConfig, string>
      SaveConfig: AppConfig -> TaskResult<unit, string> }
