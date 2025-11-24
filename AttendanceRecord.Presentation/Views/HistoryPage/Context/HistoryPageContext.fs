namespace AttendanceRecord.Presentation.Views.HistoryPage.Context

open System
open System.Threading
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.UseCases.WorkRecords

type HistoryPageContext =
    { CurrentMonth: ReactiveProperty<DateTime>
      CurrentDate: ReactiveProperty<DateTime option>
      MonthlyRecords: ReadOnlyReactiveProperty<WorkRecordListDto>
      FormCtx: FormContext<WorkRecordSaveRequestDto>
      CurrentRecord: ReadOnlyReactiveProperty<WorkRecordDetailsDto option>
      ReloadAfterSave: Guid option -> unit
      ConfirmDiscard: CancellationToken -> Tasks.Task<bool> }

type HistoryPageContextProps =
    { GetMonthlyWorkRecords: GetMonthlyWorkRecords
      GetWorkRecordDetails: GetWorkRecordDetails
      SaveWorkRecord: SaveWorkRecord
      DeleteWorkRecord: DeleteWorkRecord }

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

        let loadMonthlyRecords (month: DateTime) : unit =
            invokeTask disposables (fun ct ->
                task {
                    match! props.GetMonthlyWorkRecords.Handle month ct with
                    | Ok records -> monthlyRecords.Value <- records
                    | Error _ -> ()
                })
            |> ignore

        let loadSelectedRecord (dateOpt: DateTime option) : unit =
            match dateOpt with
            | None -> currentRecord.Value <- None
            | Some date ->
                let matchingRecord =
                    monthlyRecords.CurrentValue.WorkRecords
                    |> List.tryFind (fun r -> r.Date.Date = date.Date)

                match matchingRecord with
                | Some record ->
                    invokeTask disposables (fun ct ->
                        task {
                            match! props.GetWorkRecordDetails.Handle record.Id ct with
                            | Ok(Some details) -> currentRecord.Value <- Some details
                            | _ -> currentRecord.Value <- None
                        })
                    |> ignore
                | None -> currentRecord.Value <- None

        let reloadAfterSave (idOpt: Guid option) =
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

        let confirmDiscard _ : Tasks.Task<bool> =
            invokeTask disposables (fun ct ->
                task {
                    if formCtx.IsFormDirty.CurrentValue then
                        return!
                            MessageBox.show
                                { Title = "確認"
                                  Message = "保存されていない変更があります。\n変更を破棄してもよろしいですか？"
                                  OkContent = None
                                  CancelContent = None
                                  Buttons = MessageBoxButtons.OkCancel }
                                (Some ct)
                    else
                        return true
                })

        // Load monthly records when month changes
        currentMonth |> R3.subscribe loadMonthlyRecords |> disposables.Add

        // Load selected record when date or monthly records change
        R3.combineLatest2 selectedDate monthlyRecords
        |> R3.subscribe (fun (dateOpt, _) -> loadSelectedRecord dateOpt)
        |> disposables.Add

        // Transfer current record to form when it changes
        R3.combineLatest2 selectedDate currentRecord
        |> R3.subscribe (fun (date, rOpt) ->
            let request =
                match date, rOpt with
                | Some _, Some r -> WorkRecordSaveRequestDto.fromResponse r
                | Some date, None -> WorkRecordSaveRequestDto.empty date
                | None, _ -> WorkRecordSaveRequestDto.empty DateTime.MinValue

            formCtx.ResetForm(Some request))
        |> disposables.Add

        { CurrentMonth = currentMonth
          CurrentDate = selectedDate
          MonthlyRecords = monthlyRecords
          FormCtx = formCtx
          CurrentRecord = currentRecord
          ReloadAfterSave = reloadAfterSave
          ConfirmDiscard = confirmDiscard }
