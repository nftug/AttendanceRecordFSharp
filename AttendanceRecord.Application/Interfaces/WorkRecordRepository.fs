namespace AttendanceRecord.Application.Interfaces

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities

type WorkRecordRepository =
    { Save: WorkTimeRecord -> TaskResult<unit, string>
      Delete: Guid -> TaskResult<unit, string>
      GetByDate: DateTime -> TaskResult<WorkTimeRecord option, string>
      GetById: Guid -> TaskResult<WorkTimeRecord option, string>
      GetMonthly: DateTime -> TaskResult<WorkTimeRecord list, string> }
