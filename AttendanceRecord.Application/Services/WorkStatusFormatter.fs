namespace AttendanceRecord.Application.Services

open System
open System.Text.RegularExpressions
open AttendanceRecord.Application.Dtos.Responses

type private TimeFormatPlaceHolder =
    | Hours
    | Minutes
    | Seconds

type private FormatPlaceholder =
    | WorkTime of TimeFormatPlaceHolder
    | RestTime of TimeFormatPlaceHolder
    | Overtime of TimeFormatPlaceHolder
    | OvertimeMonthly of TimeFormatPlaceHolder
    | Unknown of string

module private FormatPlaceholder =
    let parseTimeFormat (key: string) : TimeFormatPlaceHolder =
        if key.EndsWith("hh") then Hours
        elif key.EndsWith("mm") then Minutes
        elif key.EndsWith("ss") then Seconds
        else Minutes

    let parse (key: string) : FormatPlaceholder =
        match key.ToLowerInvariant() with
        | k when k.StartsWith("work") -> WorkTime(parseTimeFormat k)
        | k when k.StartsWith("rest") -> RestTime(parseTimeFormat k)
        | k when k.StartsWith("over_monthly") -> OvertimeMonthly(parseTimeFormat k)
        | k when k.StartsWith("over") -> Overtime(parseTimeFormat k)
        | _ -> Unknown key

    let formatTimeSpan (ts: TimeSpan) (format: TimeFormatPlaceHolder) : string =
        match format with
        | Hours -> int ts.TotalHours |> string
        | Minutes -> ts.Minutes |> string
        | Seconds -> ts.Seconds |> string

    let formatValue (status: WorkStatusDto) (placeholder: FormatPlaceholder) : string =
        match placeholder with
        | WorkTime format -> formatTimeSpan status.Summary.TotalWorkTime format
        | RestTime format -> formatTimeSpan status.Summary.TotalRestTime format
        | Overtime format -> formatTimeSpan status.Summary.Overtime format
        | OvertimeMonthly format -> formatTimeSpan status.OvertimeMonthly format
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
