namespace AttendanceRecord.Application.Interfaces

open R3
open System.Threading.Tasks
open AttendanceRecord.Persistence.Dtos

type NamedPipe =
    { Receiver: Observable<NamedPipeMessage>
      SendMessage: string -> Task<Result<unit, string>> }

module NamedPipe =
    [<Literal>]
    let pipeName = "AttendanceRecordPipe"

module NamedPipeMessage =
    [<Literal>]
    let showMainWindow = "ShowMainWindow"
