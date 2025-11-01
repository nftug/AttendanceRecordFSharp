namespace AttendanceRecord.Application.Services

open System
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open R3
open AttendanceRecord.Domain.Entities
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Interfaces

type CurrentStatusStore(timerService: TimerService, repository: WorkRecordRepository, standardWorkTime: TimeSpan) as this
    =
    let disposable = new CompositeDisposable()

    let workRecord =
        (new ReactiveProperty<WorkTimeRecord option>(None)).AddTo disposable

    let monthlyWorkRecord =
        (new ReactiveProperty<WorkTimeRecord list>([])).AddTo disposable

    let currentStatus =
        workRecord
            .CombineLatest(
                monthlyWorkRecord,
                fun workRecord monthlyRecords ->
                    let monthlyOvertime = WorkTimeTally.getOvertimeTotal monthlyRecords standardWorkTime
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

    member _.WorkTime = workRecord :> ReadOnlyReactiveProperty<WorkTimeRecord option>

    member _.MonthlyWorkTime =
        monthlyWorkRecord :> ReadOnlyReactiveProperty<WorkTimeRecord list>

    member _.CurrentStatus = currentStatus

    member _.Update(forceReload: bool) : Task<Result<unit, string>> =
        taskResult {
            let today = DateTime.Now.Date

            let isRecordTodays =
                match workRecord.Value with
                | None -> false
                | Some record -> record |> WorkTimeRecord.getStartedAt = today

            let! recordOption =
                if forceReload || not isRecordTodays then
                    repository.GetByDate today
                else
                    Ok workRecord.Value |> Task.FromResult

            workRecord.Value <- recordOption

            let isMonthlyDataStale =
                monthlyWorkRecord.Value |> List.exists (not << WorkTimeRecord.isTodays)

            let! monthlyRecordsOption =
                if forceReload || isMonthlyDataStale then
                    repository.GetMonthly today
                else
                    Ok monthlyWorkRecord.Value |> Task.FromResult

            monthlyWorkRecord.Value <- monthlyRecordsOption
        }

    member _.Reload() : TaskResult<unit, string> = this.Update(forceReload = true)

    interface IDisposable with
        member _.Dispose() = disposable.Dispose()
