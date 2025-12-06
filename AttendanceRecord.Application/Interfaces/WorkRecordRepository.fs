namespace AttendanceRecord.Application.Interfaces

open System
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Domain.Entities

type WorkRecordRepository =
   { Save: WorkRecord -> CancellationToken -> TaskResult<unit, string>
     Delete: Guid -> CancellationToken -> TaskResult<unit, string>
     GetByDate: DateTime -> CancellationToken -> TaskResult<WorkRecord option, string>
     GetById: Guid -> CancellationToken -> TaskResult<WorkRecord option, string>
     GetMonthly: DateTime -> CancellationToken -> TaskResult<WorkRecord list, string> }
