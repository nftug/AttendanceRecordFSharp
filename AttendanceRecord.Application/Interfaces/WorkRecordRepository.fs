namespace AttendanceRecord.Application.Interfaces

open System
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities

type WorkRecordRepository =
    { Save: WorkRecord -> TaskResult<unit, string>
      Delete: Guid -> TaskResult<unit, string>
      GetByDate: DateTime -> TaskResult<WorkRecord option, string>
      GetById: Guid -> TaskResult<WorkRecord option, string>
      GetMonthly: DateTime -> TaskResult<WorkRecord list, string> }
