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
        (ctx: HistoryPageContext)
        (disposables: CompositeDisposable)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                match ctx.CurrentDate.CurrentValue with
                | None ->
                    ctx.CurrentStatus.Value <- None
                    ctx.Form.Value <- None
                | Some date ->
                    let matchingRecord =
                        ctx.MonthlyRecords.CurrentValue.WorkRecords
                        |> List.tryFind (fun r -> r.Date.Date = date.Date)

                    let! detailsOpt =
                        match matchingRecord with
                        | Some record -> props.GetWorkRecordDetails.Handle record.Id ct
                        | None -> task { return Ok None }

                    match detailsOpt with
                    | Ok(Some details) ->
                        ctx.CurrentStatus.Value <- Some(WorkRecordStatus.fromDetails details)
                        ctx.Form.Value <- Some(WorkRecordSaveRequestDto.fromResponse details)
                    | _ ->
                        ctx.CurrentStatus.Value <- Some(WorkRecordStatus.empty ())
                        ctx.Form.Value <- Some(WorkRecordSaveRequestDto.empty date)
            })
        |> ignore

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

            let currentStatus =
                R3.property (None: WorkRecordStatus option) |> R3.disposeWith disposables

            let isFormDirty = R3.property false |> R3.disposeWith disposables

            let ctx: HistoryPageContext =
                { CurrentMonth = currentMonth
                  CurrentDate = selectedDate
                  IsFormDirty = isFormDirty
                  MonthlyRecords = monthlyRecords
                  Form = form
                  CurrentStatus = currentStatus }

            // Load monthly records when month changes
            currentMonth
            |> R3.subscribe (loadMonthlyRecords props monthlyRecords disposables)
            |> disposables.Add

            // Load selected record when date changes
            selectedDate
            |> R3.combineLatest2 monthlyRecords (fun _ _ -> ())
            |> R3.subscribe (fun _ -> loadSelectedRecord props ctx disposables)
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
