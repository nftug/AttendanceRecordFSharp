namespace AttendanceRecord.Application.Dtos.Responses

open System
open AttendanceRecord.Domain.ValueObjects

type TimeDurationDto =
    { StartedAt: DateTime
      EndedAt: DateTime option
      Duration: TimeSpan }

module TimeDurationDto =
    let empty: TimeDurationDto =
        { StartedAt = DateTime.MinValue
          EndedAt = None
          Duration = TimeSpan.Zero }

    let fromDomain (now: DateTime) (timeDuration: TimeDuration) : TimeDurationDto =
        { StartedAt = timeDuration |> TimeDuration.getStartedAt
          EndedAt = timeDuration |> TimeDuration.getEndedAt
          Duration = timeDuration |> TimeDuration.getDuration now }
