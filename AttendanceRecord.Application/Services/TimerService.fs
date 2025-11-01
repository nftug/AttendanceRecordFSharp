namespace AttendanceRecord.Application.Services

open System
open R3

type TimerService =
    { OneSecondTimer: Observable<DateTime> }

module TimerService =
    let create () : TimerService =
        let oneSecondTimer =
            Observable
                .Interval(TimeSpan.FromSeconds 1.0)
                .Select(fun _ -> DateTime.Now)
                .Publish()
                .RefCount()

        { OneSecondTimer = oneSecondTimer }
