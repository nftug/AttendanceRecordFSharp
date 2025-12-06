namespace AttendanceRecord.Domain.Errors

open System

type WorkRecordError =
   | WorkDurationError of TimeDurationError
   | WorkRestsErrors of RestRecordError list
   | WorkGenericError of string

module WorkRecordErrors =
   let duration (error: TimeDurationError) : WorkRecordError list = [ WorkDurationError error ]

   let rest (error: RestRecordError) : WorkRecordError list = [ WorkRestsErrors [ error ] ]

   let restList (errors: RestRecordError list) : WorkRecordError list = [ WorkRestsErrors errors ]

   let generic (error: string) : WorkRecordError list = [ WorkGenericError error ]

   let chooseDuration (errors: WorkRecordError list) : string list =
      errors
      |> List.collect (function
         | WorkDurationError(TimeDurationError msg) -> [ msg ]
         | _ -> [])

   let chooseRestsDuration (errors: WorkRecordError list) : (Guid * string) list =
      errors
      |> List.collect (function
         | WorkRestsErrors restErrors -> RestRecordErrors.chooseDuration restErrors
         | _ -> [])

   let chooseRestsGeneric (errors: WorkRecordError list) : (Guid * string) list =
      errors
      |> List.collect (function
         | WorkRestsErrors restErrors -> RestRecordErrors.chooseGeneric restErrors
         | _ -> [])

   let chooseRestsAll (errors: WorkRecordError list) : (Guid * string) list =
      errors
      |> List.collect (function
         | WorkRestsErrors restErrors -> RestRecordErrors.chooseAll restErrors
         | _ -> [])

   let chooseGeneric (errors: WorkRecordError list) : string list =
      errors
      |> List.collect (function
         | WorkGenericError msg -> [ msg ]
         | _ -> [])

   let chooseDurationOrGeneric (errors: WorkRecordError list) : string list =
      errors
      |> List.collect (function
         | WorkDurationError(TimeDurationError msg) -> [ msg ]
         | WorkGenericError msg -> [ msg ]
         | _ -> [])
