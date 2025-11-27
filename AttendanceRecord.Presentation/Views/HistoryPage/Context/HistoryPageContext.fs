namespace AttendanceRecord.Presentation.Views.HistoryPage.Context

open System
open System.Threading
open R3
open FsToolkit.ErrorHandling
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.UseCases.WorkRecords
open AttendanceRecord.Application.Services

type HistoryPageContext =
    { CurrentMonth: ReactiveProperty<DateTime>
      SelectedDate: ReactiveProperty<DateTime option>
      CurrentDate: ReadOnlyReactiveProperty<DateTime>
      MonthlyRecords: Observable<WorkRecordListDto>
      FormCtx: FormContext<WorkRecordSaveRequestDto>
      CurrentSummary: Observable<WorkRecordSummaryDto option>
      SaveMutation: UseMutationResult<unit, unit>
      DeleteMutation: UseMutationResult<unit, unit>
      ConfirmDiscard: CancellationToken -> Tasks.Task<bool> }

type HistoryPageContextProps =
    { GetMonthlyWorkRecords: GetMonthlyWorkRecords
      GetWorkRecordDetails: GetWorkRecordDetails
      SaveWorkRecord: SaveWorkRecord
      DeleteWorkRecord: DeleteWorkRecord
      CurrentStatusStore: CurrentStatusStore }

[<AutoOpen>]
module HistoryPageContext =
    let createHistoryPageContext
        (props: HistoryPageContextProps)
        (disposables: CompositeDisposable)
        : HistoryPageContext =
        let now = DateTime.Now

        let currentMonth =
            R3.property (DateTime(now.Year, now.Month, 1)) |> R3.disposeWith disposables

        let selectedDate = R3.property (Some now.Date) |> R3.disposeWith disposables

        let monthlyRecords =
            R3.property (WorkRecordListDto.empty now) |> R3.disposeWith disposables

        let formCtx =
            FormContext.create (WorkRecordSaveRequestDto.empty DateTime.MinValue) disposables

        let currentRecord =
            R3.property (None: WorkRecordDetailsDto option) |> R3.disposeWith disposables

        let currentDate =
            formCtx.Form
            |> R3.map _.StartedAt.Date
            |> R3.readonly None
            |> R3.disposeWith disposables

        let currentSummary =
            R3.combineLatest3
                selectedDate
                (currentRecord |> R3.map (Option.map _.Summary))
                (props.CurrentStatusStore.CurrentStatus |> R3.map _.Summary)
            |> R3.map (fun (dateOpt, recordSummaryOpt, todaysSummary) ->
                match dateOpt with
                | Some date when date = DateTime.Today -> Some todaysSummary
                | _ -> recordSummaryOpt)

        let loadMonthlyRecords (month: DateTime) : unit =
            invokeTask disposables (fun ct ->
                task {
                    match! props.GetMonthlyWorkRecords.Handle month ct with
                    | Ok records -> monthlyRecords.Value <- records
                    | Error _ -> ()
                })
            |> ignore

        let loadSelectedRecord (dateOpt: DateTime option) : unit =
            invokeTask disposables (fun ct ->
                task {
                    match dateOpt with
                    | None -> currentRecord.Value <- None
                    | Some date ->
                        let matchingRecord =
                            monthlyRecords.CurrentValue.WorkRecords
                            |> List.tryFind (fun r -> r.Date.Date = date.Date)

                        match matchingRecord with
                        | Some record ->
                            match! props.GetWorkRecordDetails.Handle record.Id ct with
                            | Ok(Some details) -> currentRecord.Value <- Some details
                            | _ -> currentRecord.Value <- None
                        | None -> currentRecord.Value <- None
                })
            |> ignore

        let confirmDiscard _ : Tasks.Task<bool> =
            invokeTask disposables (fun ct ->
                task {
                    if formCtx.IsFormDirty.CurrentValue then
                        return!
                            Dialog.show
                                { Title = "確認"
                                  Message = "保存されていない変更があります。\n変更を破棄してもよろしいですか？"
                                  Buttons = YesNoButton(Some "破棄", Some "キャンセル") }
                                (Some ct)
                            |> Task.map (fun result -> result = YesResult)
                    else
                        return true
                })

        let reloadAfterSave (idOpt: Guid option) : unit =
            invokeTask disposables (fun ct ->
                task {
                    // 月次レコードを再読み込み
                    loadMonthlyRecords currentMonth.CurrentValue

                    // 指定されたIDのレコードを再読み込み
                    match idOpt with
                    | Some id ->
                        match! props.GetWorkRecordDetails.Handle id ct with
                        | Ok(Some details) -> currentRecord.Value <- Some details
                        | _ -> currentRecord.Value <- None
                    | None -> currentRecord.Value <- None
                })
            |> ignore

        let saveMutation =
            useMutation disposables (fun () ct ->
                task {
                    match! props.SaveWorkRecord.Handle formCtx.Form.Value ct with
                    | Ok id ->
                        Notification.show
                            { Title = "保存完了"
                              Message = "勤務記録を保存しました。"
                              NotificationType = NotificationType.Success }

                        reloadAfterSave (Some id)
                        return Ok()
                    | Error e ->
                        Notification.show
                            { Title = "保存エラー"
                              Message = $"勤務記録の保存に失敗しました: {e}"
                              NotificationType = NotificationType.Error }

                        return Error e
                })

        let deleteMutation =
            useMutation disposables (fun () ct ->
                task {
                    match formCtx.Form.Value.Id with
                    | Some id ->
                        let! shouldProceed =
                            Dialog.show
                                { Title = "削除の確認"
                                  Message = "この記録を削除してもよろしいですか？"
                                  Buttons = YesNoButton(Some "削除", Some "キャンセル") }
                                (Some ct)

                        if shouldProceed = YesResult then
                            match! props.DeleteWorkRecord.Handle id ct with
                            | Ok _ ->
                                Notification.show
                                    { Title = "削除完了"
                                      Message = "勤務記録を削除しました。"
                                      NotificationType = NotificationType.Success }

                                selectedDate.Value <- None
                                reloadAfterSave None
                                return Ok()
                            | Error e ->
                                Notification.show
                                    { Title = "削除エラー"
                                      Message = $"勤務記録の削除に失敗しました: {e}"
                                      NotificationType = NotificationType.Error }

                                return Error e
                        else
                            return Ok()
                    | _ -> return Ok()
                })

        // Load monthly records when month changes
        currentMonth |> R3.subscribe loadMonthlyRecords |> disposables.Add

        // Load selected record when date or monthly records change
        R3.combineLatest2 selectedDate monthlyRecords
        |> R3.subscribe (fun (dateOpt, _) -> loadSelectedRecord dateOpt)
        |> disposables.Add

        // Transfer current record to form when it changes
        R3.combineLatest2 selectedDate currentRecord
        |> R3.distinctUntilChanged
        |> R3.subscribe (fun (date, rOpt) ->
            let request =
                match date, rOpt with
                | Some _, Some r -> WorkRecordSaveRequestDto.fromResponse r
                | Some date, None -> WorkRecordSaveRequestDto.empty date
                | None, _ -> WorkRecordSaveRequestDto.empty DateTime.MinValue

            formCtx.ResetForm(Some request))
        |> disposables.Add

        { CurrentMonth = currentMonth
          SelectedDate = selectedDate
          CurrentDate = currentDate
          MonthlyRecords = monthlyRecords
          FormCtx = formCtx
          CurrentSummary = currentSummary
          ConfirmDiscard = confirmDiscard
          SaveMutation = saveMutation
          DeleteMutation = deleteMutation }
