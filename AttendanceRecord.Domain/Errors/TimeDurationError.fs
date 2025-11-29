namespace AttendanceRecord.Domain.Errors

type TimeDurationError =
    | StartedAtError of string
    | EndedAtError of string
