namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open Avalonia.Controls.Notifications
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.HistoryPage.Context
open AttendanceRecord.Shared
open AttendanceRecord.Application.UseCases.WorkRecords

type WorkRecordEditViewProps =
    { SaveWorkRecord: SaveWorkRecord
      DeleteWorkRecord: DeleteWorkRecord
      GetWorkRecordDetails: GetWorkRecordDetails }

[<AutoOpen>]
module private WorkRecordEditViewLogic =
    let handleSave
        (props: WorkRecordEditViewProps)
        (ctx: HistoryPageContext)
        (disposables: R3.CompositeDisposable)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                match ctx.Form.Value with
                | Some request ->
                    let! result = props.SaveWorkRecord.Handle request ct

                    match result with
                    | Ok() ->
                        Notification.show "保存完了" "勤務記録を保存しました。" NotificationType.Information
                        ctx.IsFormDirty.Value <- false

                        // Reload the saved record details if we have an ID
                        match request.Id with
                        | Some id ->
                            match! props.GetWorkRecordDetails.Handle id ct with
                            | Ok(Some updatedDetails) ->
                                ctx.Form.Value <-
                                    Some(WorkRecordSaveRequestDto.fromResponse updatedDetails)

                                ctx.CurrentStatus.Value <-
                                    Some(WorkRecordStatus.fromDetails updatedDetails)
                            | _ -> ()
                        | None -> ()

                    | Error e ->
                        Notification.show "保存エラー" $"勤務記録の保存に失敗しました: {e}" NotificationType.Error
                | None -> ()
            })
        |> ignore

    let handleDelete
        (props: WorkRecordEditViewProps)
        (ctx: HistoryPageContext)
        (disposables: R3.CompositeDisposable)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                match ctx.Form.Value with
                | Some r when r.Id.IsSome ->
                    let! shouldDelete =
                        MessageBox.show
                            { Title = "削除の確認"
                              Message = "この記録を削除してもよろしいですか?"
                              OkContent = Some "削除"
                              CancelContent = None
                              Buttons = MessageBoxButtons.OkCancel }
                            (Some ct)

                    if shouldDelete then
                        let! result = props.DeleteWorkRecord.Handle r.Id.Value ct

                        match result with
                        | Ok _ ->
                            Notification.show "削除完了" "勤務記録を削除しました。" NotificationType.Information
                            ctx.IsFormDirty.Value <- false
                            ctx.CurrentDate.Value <- None
                        | Error e ->
                            Notification.show "削除エラー" $"勤務記録の削除に失敗しました: {e}" NotificationType.Error
                | _ -> ()
            })
        |> ignore

module WorkRecordEditView =
    open NXUI.Extensions
    open type NXUI.Builders

    let private createRecordView
        (editingRecord: R3.ReactiveProperty<WorkRecordSaveRequestDto option>)
        (props: WorkRecordEditViewProps)
        =
        editingRecord
        |> toViewWithReactive (fun record disposables self ->
            match record with
            | None ->
                TextBlock()
                    .Text("日付を選択してください")
                    .FontSize(16.0)
                    .HorizontalAlignmentCenter()
                    .VerticalAlignmentCenter()
            | Some record ->
                let ctx, _ = HistoryPageContextProvider.require self

                let actionButtons =
                    StackPanel()
                        .OrientationHorizontal()
                        .Spacing(10.0)
                        .Margin(10.0)
                        .Children(
                            Button()
                                .Content("保存")
                                .OnClickHandler(fun _ _ -> handleSave props ctx disposables)
                                .Width(100.0)
                                .Height(35.0),
                            Button()
                                .Content("削除")
                                .OnClickHandler(fun _ _ -> handleDelete props ctx disposables)
                                .Width(100.0)
                                .Height(35.0)
                                .IsEnabled(record.Id.IsSome)
                        )

                DockPanel()
                    .LastChildFill(true)
                    .Children(
                        Grid()
                            .ColumnDefinitions("*,Auto")
                            .DockBottom()
                            .Children(actionButtons.Column(1)),
                        ScrollViewer()
                            .Content(
                                StackPanel()
                                    .Margin(20.0)
                                    .Spacing(20.0)
                                    .Children(
                                        TextBlock()
                                            .Text(record.StartedAt.ToString "yyyy/MM/dd")
                                            .FontSize(24.0)
                                            .FontWeightBold(),
                                        WorkStatusSummarySection.createSummarySection (),
                                        WorkTimeSection.create (),
                                        RestTimeSection.create ()
                                    )
                            )
                    ))

    let create (props: WorkRecordEditViewProps) : Avalonia.Controls.Control =
        withReactive (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self

            let editingRecord =
                R3.property (None: WorkRecordSaveRequestDto option)
                |> R3.disposeWith disposables

            // Sync editingRecord with selectedRecord
            ctx.Form
            |> R3.subscribe (fun record ->
                editingRecord.Value <- record
                ctx.IsFormDirty.Value <- false)
            |> disposables.Add

            createRecordView editingRecord props)
