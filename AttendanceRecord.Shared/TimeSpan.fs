namespace AttendanceRecord.Shared

open System

module TimeSpan =
    let formatDuration (duration: TimeSpan) =
        let format = @"hh\:mm\:ss"

        if duration < TimeSpan.Zero then
            $"-{duration.ToString format}"
        else
            $"{duration.ToString format}"
