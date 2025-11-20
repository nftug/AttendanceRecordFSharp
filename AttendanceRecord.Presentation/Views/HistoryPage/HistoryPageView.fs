namespace AttendanceRecord.Presentation.Views.HistoryPage

open System
open System.Threading
open System.Threading.Tasks
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Application.Dtos.Responses
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
        (monthlyRecordsValue: WorkRecordListDto)
        (selectedRecord: ReactiveProperty<WorkRecordDetailsDto option>)
        (disposables: CompositeDisposable)
        (dateOpt: DateTime option)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                match dateOpt with
                | None -> selectedRecord.Value <- None
                | Some date ->
                    let matchingRecord =
                        monthlyRecordsValue.WorkRecords
                        |> List.tryFind (fun r -> r.Date.Date = date.Date)

                    match matchingRecord with
                    | Some record ->
                        match! props.GetWorkRecordDetails.Handle record.Id ct with
                        | Ok(Some details) -> selectedRecord.Value <- Some details
                        | _ -> selectedRecord.Value <- None
                    | None ->
                        // 新規作成用の空レコード
                        selectedRecord.Value <-
                            Some
                                { Id = Guid.Empty
                                  Date = date
                                  WorkTimeDuration =
                                    { StartedAt = date
                                      EndedAt = None
                                      Duration = TimeSpan.Zero }
                                  RestTimes = []
                                  WorkTime = TimeSpan.Zero
                                  RestTime = TimeSpan.Zero
                                  Overtime = TimeSpan.Zero
                                  IsActive = false
                                  IsWorking = false
                                  IsResting = false }
            })
        |> ignore

[<AutoOpen>]
module private HistoryPageViewLogic =
    open AttendanceRecord.Presentation.Views.Common

    let confirmDiscard (isFormDirty: ReactiveProperty<bool>) (ct: CancellationToken) : Task<bool> =
        task {
            if not isFormDirty.Value then
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
                R3.property
                    { MonthDate = DateTime(now.Year, now.Month, 1)
                      WorkRecords = []
                      WorkTimeTotal = TimeSpan.Zero
                      RestTimeTotal = TimeSpan.Zero
                      OvertimeTotal = TimeSpan.Zero }
                |> R3.disposeWith disposables

            let selectedRecord =
                R3.property (None: WorkRecordDetailsDto option) |> R3.disposeWith disposables

            let isFormDirty = R3.property false |> R3.disposeWith disposables

            // Load monthly records when month changes
            currentMonth
            |> R3.subscribe (loadMonthlyRecords props monthlyRecords disposables)
            |> disposables.Add

            // Load selected record when date changes
            selectedDate
            |> R3.combineLatest2 monthlyRecords (fun d m -> d, m)
            |> R3.subscribe (fun (dateOpt, monthlyRecordsValue) ->
                loadSelectedRecord props monthlyRecordsValue selectedRecord disposables dateOpt)
            |> disposables.Add

            let context: HistoryPageContext =
                { CurrentMonth = currentMonth
                  CurrentDate = selectedDate
                  MonthlyRecords = monthlyRecords
                  SelectedRecord = selectedRecord
                  OnSaveRecord = props.SaveWorkRecord.Handle
                  OnDeleteRecord = props.DeleteWorkRecord.Handle }

            let toolbarProps: HistoryToolbarProps =
                { OnConfirmDiscard = confirmDiscard isFormDirty }

            let listViewProps: WorkRecordListViewProps =
                { OnConfirmDiscard = confirmDiscard isFormDirty }

            let editViewProps: WorkRecordEditViewProps =
                { OnSave = props.SaveWorkRecord.Handle
                  OnDelete = props.DeleteWorkRecord.Handle
                  IsDirty = isFormDirty
                  OnRequestDateChange = fun _ ct -> confirmDiscard isFormDirty ct }

            HistoryPageContextProvider.provide
                context
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
                                    .Width(5.0)
                                    .Background(Avalonia.Media.Brushes.LightGray)
                                    .ResizeDirectionColumns(),
                                (WorkRecordEditView.create editViewProps).Column(2)
                            )
                    )))
