namespace AttendanceRecord.Application.UseCases.AppConfig

open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Domain.Entities

type GetAppConfig = { Handle: unit -> AppConfigDto }

module GetAppConfig =
    let private handle (getAppConfig: unit -> AppConfig) =
        getAppConfig () |> AppConfigDto.fromDomain

    let create (getAppConfig: unit -> AppConfig) : GetAppConfig =
        { Handle = fun () -> handle getAppConfig }
