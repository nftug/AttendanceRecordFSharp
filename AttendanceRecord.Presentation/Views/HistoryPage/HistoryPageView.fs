namespace AttendanceRecord.Presentation.Views.HistoryPage

open System
open System.Threading
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Presentation.Views.HistoryPage.Edit
open AttendanceRecord.Presentation.Views.HistoryPage.Navigation
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.UseCases.WorkRecords

type HistoryPageProps =
    { GetMonthlyWorkRecords: GetMonthlyWorkRecords
      GetWorkRecordDetails: GetWorkRecordDetails
      SaveWorkRecord: SaveWorkRecord
      DeleteWorkRecord: DeleteWorkRecord }

[<AutoOpen>]
module private HistoryPageLogic =
    let loadMonthlyRecords
        (props: HistoryPageProps)
        (monthlyRecords: ReactiveProperty<WorkRecordListDto>)
        (disposables: CompositeDisposable)
        (month: DateTime)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                match! props.GetMonthlyWorkRecords.Handle month ct with
                | Ok records -> monthlyRecords.Value <- records
                | Error _ -> ()
            })
        |> ignore

    let loadSelectedRecord
        (props: HistoryPageProps)
        (currentRecord: ReactiveProperty<WorkRecordDetailsDto option>)
        (monthlyRecords: WorkRecordListDto)
        (disposables: CompositeDisposable)
        (dateOpt: DateTime option)
        : unit =
        match dateOpt with
        | None -> currentRecord.Value <- None
        | Some date ->
            let matchingRecord =
                monthlyRecords.WorkRecords |> List.tryFind (fun r -> r.Date.Date = date.Date)

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

    let confirmDiscard
        (isFormDirty: ReadOnlyReactiveProperty<bool>)
        (ct: CancellationToken)
        : Tasks.Task<bool> =
        task {
            if not isFormDirty.CurrentValue then
                return true
            else
                return!
                    MessageBox.show
                        { Title = "確認"
                          Message = "保存されていない変更があります。このまま移動しますか?"
                          OkContent = None
                          CancelContent = None
                          Buttons = MessageBoxButtons.OkCancel }
                        (Some ct)
        }

    let reloadAfterSave
        (props: HistoryPageProps)
        (currentRecord: ReactiveProperty<WorkRecordDetailsDto option>)
        (monthlyRecords: ReactiveProperty<WorkRecordListDto>)
        (currentMonth: ReadOnlyReactiveProperty<DateTime>)
        (disposables: CompositeDisposable)
        (idOpt: Guid option)
        =
        invokeTask disposables (fun ct ->
            task {
                // 月次レコードを再読み込み
                loadMonthlyRecords props monthlyRecords disposables currentMonth.CurrentValue

                // 指定されたIDのレコードを再読み込み
                match idOpt with
                | Some id ->
                    match! props.GetWorkRecordDetails.Handle id ct with
                    | Ok(Some details) -> currentRecord.Value <- Some details
                    | _ -> currentRecord.Value <- None
                | None -> currentRecord.Value <- None
            })
        |> ignore

module HistoryPageView =
    open NXUI.Extensions
    open type NXUI.Builders

    let create (props: HistoryPageProps) : Avalonia.Controls.Control =
        withReactive (fun disposables _ ->
            let now = DateTime.Now

            let currentMonth =
                R3.property (DateTime(now.Year, now.Month, 1)) |> R3.disposeWith disposables

            let selectedDate = R3.property (Some now.Date) |> R3.disposeWith disposables

            let monthlyRecords =
                R3.property (WorkRecordListDto.empty now) |> R3.disposeWith disposables

            let form =
                R3.property (None: WorkRecordSaveRequestDto option)
                |> R3.disposeWith disposables

            let currentRecord =
                R3.property (None: WorkRecordDetailsDto option) |> R3.disposeWith disposables

            let isFormDirty = R3.property false |> R3.disposeWith disposables

            let reloadAfterSave =
                reloadAfterSave props currentRecord monthlyRecords currentMonth disposables

            let ctx: HistoryPageContext =
                { CurrentMonth = currentMonth
                  CurrentDate = selectedDate
                  IsFormDirty = isFormDirty
                  MonthlyRecords = monthlyRecords
                  Form = form
                  CurrentRecord = currentRecord
                  ReloadAfterSave = reloadAfterSave }

            // Load monthly records when month changes
            currentMonth
            |> R3.subscribe (loadMonthlyRecords props monthlyRecords disposables)
            |> disposables.Add

            // Load selected record when date or monthly records change
            selectedDate
            |> R3.combineLatest2 monthlyRecords (fun date records -> date, records)
            |> R3.subscribe (fun (date, records) ->
                loadSelectedRecord props currentRecord records disposables date)
            |> disposables.Add

            // Transfer current record to form when it changes
            selectedDate
            |> R3.combineLatest2 currentRecord (fun date rOpt -> date, rOpt)
            |> R3.subscribe (fun (date, rOpt) ->
                form.Value <-
                    match date, rOpt with
                    | Some _, Some r -> Some(WorkRecordSaveRequestDto.fromResponse r)
                    | Some date, None -> Some(WorkRecordSaveRequestDto.empty date)
                    | None, _ -> None)
            |> disposables.Add

            let toolbarProps: HistoryToolbarProps =
                { OnConfirmDiscard = confirmDiscard isFormDirty }

            let listViewProps: WorkRecordListViewProps =
                { OnConfirmDiscard = confirmDiscard isFormDirty }

            let editViewProps: WorkRecordEditViewProps =
                { SaveWorkRecord = props.SaveWorkRecord
                  DeleteWorkRecord = props.DeleteWorkRecord
                  GetWorkRecordDetails = props.GetWorkRecordDetails }

            HistoryPageContextProvider.provide
                ctx
                (DockPanel()
                    .LastChildFill(true)
                    .Children(
                        (HistoryToolbar.create toolbarProps).DockTop(),
                        Grid()
                            .ColumnDefinitions("250,5,*")
                            .Children(
                                (WorkRecordListView.create listViewProps).Column(0),
                                GridSplitter()
                                    .Column(1)
                                    .Width(1.0)
                                    .Background(Avalonia.Media.Brushes.DimGray)
                                    .ResizeDirectionColumns(),
                                (WorkRecordEditView.create editViewProps).Column(2)
                            )
                    )))
