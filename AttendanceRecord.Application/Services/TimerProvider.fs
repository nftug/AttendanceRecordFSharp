namespace AttendanceRecord.Application.Services

open System
open R3

type TimerProvider =
   { OneSecondTimer: Observable<DateTime> }

module TimerProvider =
   let create () : TimerProvider =
      let oneSecondTimer =
         Observable
            .Interval(TimeSpan.FromSeconds 1.0)
            .Select(fun _ -> DateTime.Now)
            .Publish()
            .RefCount()

      { OneSecondTimer = oneSecondTimer }
