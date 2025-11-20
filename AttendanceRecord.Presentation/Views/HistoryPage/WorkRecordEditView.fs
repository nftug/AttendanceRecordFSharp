namespace AttendanceRecord.Presentation.Views.HistoryPage

open System
open System.Threading
open System.Threading.Tasks
open Avalonia.Controls.Notifications
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Shared

type WorkRecordEditViewProps =
    { OnSave: WorkRecordSaveRequestDto -> CancellationToken -> Task<Result<unit, string>>
      OnDelete: Guid -> CancellationToken -> Task<Result<unit, string>>
      OnRequestDateChange: DateTime option -> CancellationToken -> Task<bool> }

[<AutoOpen>]
module private WorkRecordEditViewLogic =
    let handleSave
        (editingRecord: R3.ReactiveProperty<WorkRecordDetailsDto option>)
        (props: WorkRecordEditViewProps)
        (ctx: HistoryPageContext)
        (disposables: R3.CompositeDisposable)
        ()
        : unit =
        invokeTask disposables (fun ct ->
            task {
                match editingRecord.Value with
                | Some r ->

                    let request: WorkRecordSaveRequestDto =
                        { Id = if r.Id = Guid.Empty then None else Some r.Id
                          StartedAt = r.WorkTimeDuration.StartedAt
                          EndedAt = r.WorkTimeDuration.EndedAt
                          RestRecords =
                            r.RestTimes
                            |> List.map (fun rt ->
                                { Id = if rt.Id = Guid.Empty then None else Some rt.Id
                                  StartedAt = rt.Duration.StartedAt
                                  EndedAt = rt.Duration.EndedAt }) }

                    let! result = props.OnSave request ct

                    match result with
                    | Ok _ ->
                        Notification.show "保存完了" "勤務記録を保存しました。" NotificationType.Information
                        ctx.IsFormDirty.Value <- false
                    | Error e ->
                        Notification.show "保存エラー" $"勤務記録の保存に失敗しました: {e}" NotificationType.Error
                | None -> ()
            })
        |> ignore

    let handleDelete
        (editingRecord: R3.ReactiveProperty<WorkRecordDetailsDto option>)
        (props: WorkRecordEditViewProps)
        (ctx: HistoryPageContext)
        (disposables: R3.CompositeDisposable)
        ()
        : unit =
        invokeTask disposables (fun ct ->
            task {
                match editingRecord.Value with
                | Some r when r.Id <> Guid.Empty ->
                    let! shouldDelete =
                        MessageBox.show
                            { Title = "削除の確認"
                              Message = "この記録を削除してもよろしいですか?"
                              OkContent = Some "削除"
                              CancelContent = None
                              Buttons = MessageBoxButtons.OkCancel }
                            (Some ct)

                    if shouldDelete then
                        let! result = props.OnDelete r.Id ct

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
    open Avalonia.Media

    let private createSummaryInfoRow (label: string) (value: string) =
        StackPanel()
            .OrientationHorizontal()
            .Children(
                Label().Content(label).FontSize(14.0).FontWeightBold().Width(120.0),
                Label().Content(value).FontSize(14.0).VerticalAlignmentCenter()
            )

    let private createSummarySection (record: WorkRecordDetailsDto) =
        Border()
            .BorderThickness(1.0)
            .BorderBrush(Brushes.Gray)
            .Padding(15.0)
            .Child(
                StackPanel()
                    .Spacing(8.0)
                    .Children(
                        createSummaryInfoRow "勤務時間: " (TimeSpan.formatDuration record.WorkTime),
                        createSummaryInfoRow "休憩時間: " (TimeSpan.formatDuration record.RestTime),
                        createSummaryInfoRow "残業時間: " (TimeSpan.formatDuration record.Overtime)
                    )
            )

    let private createRecordView
        (editingRecord: R3.ReactiveProperty<WorkRecordDetailsDto option>)
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
                                .OnClickHandler(fun _ _ ->
                                    handleSave editingRecord props ctx disposables ())
                                .Width(100.0)
                                .Height(35.0),
                            Button()
                                .Content("削除")
                                .OnClickHandler(fun _ _ ->
                                    handleDelete editingRecord props ctx disposables ())
                                .Width(100.0)
                                .Height(35.0)
                                .IsEnabled(record.Id <> Guid.Empty)
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
                                            .Text(record.Date.ToString "yyyy/MM/dd")
                                            .FontSize(24.0)
                                            .FontWeightBold(),
                                        createSummarySection record,
                                        WorkTimeSection.create editingRecord,
                                        RestTimeSection.create editingRecord
                                    )
                            )
                    ))

    let create (props: WorkRecordEditViewProps) : Avalonia.Controls.Control =
        withReactive (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self

            let editingRecord =
                R3.property (None: WorkRecordDetailsDto option) |> R3.disposeWith disposables

            // Sync editingRecord with selectedRecord
            ctx.SelectedRecord
            |> R3.subscribe (fun record ->
                editingRecord.Value <- record
                ctx.IsFormDirty.Value <- false)
            |> disposables.Add

            createRecordView editingRecord props)
