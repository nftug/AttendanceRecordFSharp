namespace AttendanceRecord.Application.Services

open System
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open R3
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Interfaces

type CurrentStatusStore(timerService: TimerProvider, repository: WorkRecordRepository, standardWorkTime: TimeSpan) as this
    =
    let disposable = new CompositeDisposable()

    let workRecord = (new ReactiveProperty<WorkRecord option>(None)).AddTo disposable

    let monthlyRecords = (new ReactiveProperty<WorkRecord list>([])).AddTo disposable

    let currentStatus =
        workRecord
            .CombineLatest(
                monthlyRecords,
                fun workRecord monthlyRecords ->
                    let now = DateTime.Now

                    monthlyRecords
                    |> WorkRecordTally.getOvertimeTotal now standardWorkTime
                    |> CurrentStatusDto.fromDomain now standardWorkTime workRecord
            )
            .ToReadOnlyReactiveProperty(CurrentStatusDto.getEmpty ())
            .AddTo(disposable)

    do
        timerService.OneSecondTimer
            .Prepend(DateTime.Now)
            .Subscribe(fun _ -> this.Update false |> ignore)
            .AddTo(disposable)
        |> ignore

        this.Update true |> ignore

    member _.WorkTime = workRecord :> ReadOnlyReactiveProperty<WorkRecord option>

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
                    repository.GetByDate today
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
                    repository.GetMonthly today
                else
                    Ok monthlyRecords.Value |> Task.FromResult

            monthlyRecords.Value <- monthlyRecordsOption
            monthlyRecords.ForceNotify()
        }

    member _.Reload() : TaskResult<unit, string> = this.Update(forceReload = true)

    interface IDisposable with
        member _.Dispose() = disposable.Dispose()
