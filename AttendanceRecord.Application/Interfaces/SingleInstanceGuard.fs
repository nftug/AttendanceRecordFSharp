namespace AttendanceRecord.Application.Interfaces

type SingleInstanceGuard =
    { TryAcquireLock: unit -> bool
      ReleaseLock: unit -> unit }
