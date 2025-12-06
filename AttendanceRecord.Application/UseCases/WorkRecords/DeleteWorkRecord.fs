namespace AttendanceRecord.Application.UseCases.WorkRecords

open System
open System.Threading
open FsToolkit.ErrorHandling
open AttendanceRecord.Application.Interfaces
open AttendanceRecord.Application.Services

type DeleteWorkRecord =
   { Handle: Guid -> CancellationToken -> TaskResult<unit, string> }

module DeleteWorkRecord =
   let private handle
      (repository: WorkRecordRepository)
      (workStatusStore: WorkStatusStore)
      (id: Guid)
      (ct: CancellationToken)
      =
      taskResult {
         let! existingWorkRecordOption = repository.GetById id ct

         if existingWorkRecordOption.IsNone then
            return! Error $"勤務記録が見つかりません。"

         do! repository.Delete id ct
         do! workStatusStore.Reload()
      }

   let create
      (repository: WorkRecordRepository)
      (workStatusStore: WorkStatusStore)
      : DeleteWorkRecord =
      { Handle = fun id ct -> handle repository workStatusStore id ct }
