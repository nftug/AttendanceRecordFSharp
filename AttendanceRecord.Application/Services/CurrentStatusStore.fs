namespace AttendanceRecord.Application.Services

open System
open System.Threading
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Interfaces

type CurrentStatusStore
    (timerService: TimerProvider, repository: WorkRecordRepository, appConfig: Observable<AppConfig>) as this
    =
    let disposable = new CompositeDisposable()

    let workRecord = R3.property (None: WorkRecord option) |> R3.disposeWith disposable

    let monthlyRecords = R3.property ([]: WorkRecord list) |> R3.disposeWith disposable

    let currentStatus =
        R3.combineLatest3 workRecord monthlyRecords appConfig
        |> R3.map (fun (workRecord, monthlyRecords, appConfig) ->
            let now = DateTime.Now
            let standardWorkTime = appConfig.StandardWorkTime

            monthlyRecords
            |> WorkRecordTally.getOvertimeTotal now standardWorkTime
            |> CurrentStatusDto.fromDomain now standardWorkTime workRecord)
        |> R3.readonly (CurrentStatusDto.getEmpty () |> Some)
        |> R3.disposeWith disposable

    do
        timerService.OneSecondTimer
        |> R3.prepend DateTime.Now
        |> R3.subscribe (fun _ -> this.Update false |> ignore)
        |> disposable.Add

        this.Update true |> ignore

    member _.WorkRecord = workRecord :> ReadOnlyReactiveProperty<WorkRecord option>

    member _.MonthlyRecords = monthlyRecords :> ReadOnlyReactiveProperty<WorkRecord list>

    member _.CurrentStatus = currentStatus

    member _.Update(forceReload: bool) : Task<Result<unit, string>> =
        taskResult {
            let today = DateTime.Now.Date

            let isWorkRecordStale =
                match workRecord.Value with
                | None -> false
                | Some record -> record |> (not << WorkRecord.hasDate today)

            let! recordOption =
                if forceReload || isWorkRecordStale then
                    repository.GetByDate today CancellationToken.None
                else
                    Ok workRecord.Value |> Task.FromResult

            workRecord.Value <- recordOption
            workRecord.ForceNotify()

            let isMonthlyDataStale =
                monthlyRecords.Value
                |> List.exists (fun wr ->
                    WorkRecord.getDate wr
                    |> fun dt -> dt.Year <> today.Year || dt.Month <> today.Month)

            let! monthlyRecordsOption =
                if forceReload || isMonthlyDataStale then
                    repository.GetMonthly today CancellationToken.None
                else
                    Ok monthlyRecords.Value |> Task.FromResult

            monthlyRecords.Value <- monthlyRecordsOption
            monthlyRecords.ForceNotify()
        }

    member _.Reload() : TaskResult<unit, string> = this.Update(forceReload = true)

    interface IDisposable with
        member _.Dispose() = disposable.Dispose()
