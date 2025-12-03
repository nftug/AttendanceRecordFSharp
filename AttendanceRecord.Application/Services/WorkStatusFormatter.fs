namespace AttendanceRecord.Application.Services

open System
open System.Text.RegularExpressions
open AttendanceRecord.Application.Dtos.Responses

type TimeFormatPlaceHolder =
    | Hours
    | Minutes
    | Seconds

type TimeType =
    | WorkTime
    | RestTime
    | Overtime
    | OvertimeMonthly

type FormatPlaceholder =
    | TimeFormat of TimeType * TimeFormatPlaceHolder
    | Unknown of string

module private FormatPlaceholder =
    let parseTimeFormat (key: string) : TimeFormatPlaceHolder =
        if key.EndsWith("hh") then Hours
        elif key.EndsWith("mm") then Minutes
        elif key.EndsWith("ss") then Seconds
        else Minutes

    let parse (key: string) : FormatPlaceholder =
        match key.ToLowerInvariant() with
        | k when k.StartsWith("work") -> TimeFormat(WorkTime, parseTimeFormat k)
        | k when k.StartsWith("rest") -> TimeFormat(RestTime, parseTimeFormat k)
        | k when k.StartsWith("over_monthly") -> TimeFormat(OvertimeMonthly, parseTimeFormat k)
        | k when k.StartsWith("over") -> TimeFormat(Overtime, parseTimeFormat k)
        | _ -> Unknown key

    let formatTimeSpan (ts: TimeSpan) (format: TimeFormatPlaceHolder) : string =
        match format with
        | Hours -> int ts.TotalHours |> string
        | Minutes -> abs ts.Minutes |> string
        | Seconds -> abs ts.Seconds |> string

    let formatValue (status: WorkStatusDto) (placeholder: FormatPlaceholder) : string =
        match placeholder with
        | TimeFormat(WorkTime, format) -> formatTimeSpan status.Summary.TotalWorkTime format
        | TimeFormat(RestTime, format) -> formatTimeSpan status.Summary.TotalRestTime format
        | TimeFormat(Overtime, format) -> formatTimeSpan status.Summary.Overtime format
        | TimeFormat(OvertimeMonthly, format) -> formatTimeSpan status.OvertimeMonthly format
        | Unknown key -> "{" + key + "}"

module WorkStatusFormatter =
    let format (template: string) (status: WorkStatusDto) : string =
        let pattern = @"\{([^}]+)\}"

        Regex.Replace(
            template,
            pattern,
            (fun (m: Match) ->
                let key = m.Groups.[1].Value
                key |> FormatPlaceholder.parse |> FormatPlaceholder.formatValue status)
        )

    let toFormatString (placeholder: FormatPlaceholder) : string =
        let ofTimeFormat (format: TimeFormatPlaceHolder) : string =
            match format with
            | Hours -> "_hh"
            | Minutes -> "_mm"
            | Seconds -> "_ss"

        match placeholder with
        | TimeFormat(WorkTime, format) -> "{work" + ofTimeFormat format + "}"
        | TimeFormat(RestTime, format) -> "{rest" + ofTimeFormat format + "}"
        | TimeFormat(Overtime, format) -> "{over" + ofTimeFormat format + "}"
        | TimeFormat(OvertimeMonthly, format) -> "{over_monthly" + ofTimeFormat format + "}"
        | Unknown key -> "{" + key + "}"
