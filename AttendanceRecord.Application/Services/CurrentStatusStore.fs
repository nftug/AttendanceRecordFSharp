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
        timerService.OneSecondTimer
            .CombineLatest(
                workRecord,
                monthlyRecords,
                fun _ workRecord monthlyRecords ->
                    let monthlyOvertime =
                        WorkRecordTally.getOvertimeTotal monthlyRecords standardWorkTime

                    CurrentStatusDto.fromDomain standardWorkTime monthlyOvertime workRecord
            )
            .ToReadOnlyReactiveProperty(CurrentStatusDto.getEmpty ())
            .AddTo(disposable)

    do
        timerService.OneSecondTimer
            .Prepend(DateTime.Now)
            .Subscribe(fun _ -> this.Update false |> ignore)
            .AddTo(disposable)
        |> ignore

    member _.WorkTime = workRecord :> ReadOnlyReactiveProperty<WorkRecord option>

    member _.MonthlyRecords = monthlyRecords :> ReadOnlyReactiveProperty<WorkRecord list>

    member _.CurrentStatus = currentStatus

    member _.Update(forceReload: bool) : Task<Result<unit, string>> =
        taskResult {
            let today = DateTime.Now.Date

            let isRecordTodays =
                match workRecord.Value with
                | None -> false
                | Some record -> record |> WorkRecord.getStartedAt = today

            let! recordOption =
                if forceReload || not isRecordTodays then
                    repository.GetByDate today
                else
                    Ok workRecord.Value |> Task.FromResult

            workRecord.Value <- recordOption

            let isMonthlyDataStale =
                monthlyRecords.Value |> List.exists (not << WorkRecord.isTodays)

            let! monthlyRecordsOption =
                if forceReload || isMonthlyDataStale then
                    repository.GetMonthly today
                else
                    Ok monthlyRecords.Value |> Task.FromResult

            monthlyRecords.Value <- monthlyRecordsOption
        }

    member _.Reload() : TaskResult<unit, string> = this.Update(forceReload = true)

    interface IDisposable with
        member _.Dispose() = disposable.Dispose()
