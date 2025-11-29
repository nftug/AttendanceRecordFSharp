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

type WorkStatusStoreDependencies =
    { TimerProvider: TimerProvider
      WorkRecordRepository: WorkRecordRepository
      AppConfig: Observable<AppConfig>
      Disposables: CompositeDisposable }

type WorkStatusStore =
    { WorkRecord: ReadOnlyReactiveProperty<WorkRecord option>
      MonthlyRecords: ReadOnlyReactiveProperty<WorkRecord list>
      WorkStatus: ReadOnlyReactiveProperty<WorkStatusDto>
      Update: bool -> Task<Result<unit, string>>
      Reload: unit -> TaskResult<unit, string> }

module WorkStatusStore =
    let create (deps: WorkStatusStoreDependencies) : WorkStatusStore =
        let workRecord =
            R3.property (None: WorkRecord option) |> R3.disposeWith deps.Disposables

        let monthlyRecords =
            R3.property ([]: WorkRecord list) |> R3.disposeWith deps.Disposables

        let workStatus =
            R3.combineLatest3 workRecord monthlyRecords deps.AppConfig
            |> R3.map (fun (workRecord, monthlyRecords, appConfig) ->
                let now = DateTime.Now
                let standardWorkTime = appConfig.StandardWorkTime |> StandardWorkTime.value

                monthlyRecords
                |> WorkRecordTally.getOvertimeTotal now standardWorkTime
                |> WorkStatusDto.fromDomain now standardWorkTime workRecord)
            |> R3.readonly (WorkStatusDto.getEmpty () |> Some)
            |> R3.disposeWith deps.Disposables

        let update (forceReload: bool) : Task<Result<unit, string>> =
            taskResult {
                let today = DateTime.Now.Date

                let isWorkRecordStale =
                    match workRecord.Value with
                    | None -> false
                    | Some record -> record |> (not << WorkRecord.hasDate today)

                let! recordOption =
                    if forceReload || isWorkRecordStale then
                        deps.WorkRecordRepository.GetByDate today CancellationToken.None
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
                        deps.WorkRecordRepository.GetMonthly today CancellationToken.None
                    else
                        Ok monthlyRecords.Value |> Task.FromResult

                monthlyRecords.Value <- monthlyRecordsOption
                monthlyRecords.ForceNotify()
            }

        deps.TimerProvider.OneSecondTimer
        |> R3.prepend DateTime.Now
        |> R3.subscribe (fun _ -> update false |> ignore)
        |> deps.Disposables.Add

        update true |> ignore

        { WorkRecord = workRecord
          MonthlyRecords = monthlyRecords
          WorkStatus = workStatus
          Update = update
          Reload = fun () -> update true }
