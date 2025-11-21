namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open Avalonia.Controls.Notifications
open Material.Icons
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
                    | Ok id ->
                        Notification.show "保存完了" "勤務記録を保存しました。" NotificationType.Information
                        ctx.ReloadAfterSave(Some id)
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
                            ctx.CurrentDate.Value <- None
                            ctx.ReloadAfterSave None
                        | Error e ->
                            Notification.show "削除エラー" $"勤務記録の削除に失敗しました: {e}" NotificationType.Error
                | _ -> ()
            })
        |> ignore

module WorkRecordEditView =
    open NXUI.Extensions
    open type NXUI.Builders

    let create (props: WorkRecordEditViewProps) : Avalonia.Controls.Control =
        withReactive (fun disposables self ->
            let ctx, _ = HistoryPageContextProvider.require self

            ctx.Form
            |> toView (function
                | None ->
                    TextBlock()
                        .Text("日付を選択してください")
                        .FontSize(16.0)
                        .HorizontalAlignmentCenter()
                        .VerticalAlignmentCenter()
                | Some form ->
                    let ctx, _ = HistoryPageContextProvider.require self

                    let actionButtons =
                        StackPanel()
                            .OrientationHorizontal()
                            .Spacing(10.0)
                            .Margin(10.0)
                            .Children(
                                Button()
                                    .Content(
                                        MaterialIconLabel.create MaterialIconKind.ContentSave "保存"
                                    )
                                    .OnClickHandler(fun _ _ -> handleSave props ctx disposables)
                                    .Width(100.0)
                                    .Height(35.0),
                                Button()
                                    .Content(MaterialIconLabel.create MaterialIconKind.Delete "削除")
                                    .OnClickHandler(fun _ _ -> handleDelete props ctx disposables)
                                    .Width(100.0)
                                    .Height(35.0)
                                    .IsEnabled(form.Id.IsSome)
                            )

                    let scrollContent =
                        StackPanel()
                            .Margin(20.0)
                            .Spacing(20.0)
                            .Children(
                                TextBlock()
                                    .Text(form.StartedAt.ToString "yyyy/MM/dd (ddd)")
                                    .FontSize(28.0)
                                    .FontWeightBold(),
                                WorkStatusSummarySection.create (),
                                WorkTimeSection.create (),
                                RestTimeSection.create ()
                            )

                    DockPanel()
                        .LastChildFill(true)
                        .Children(
                            Grid()
                                .ColumnDefinitions("*,Auto")
                                .DockBottom()
                                .Children(actionButtons.Column(1)),
                            ScrollViewer().Content(scrollContent)
                        )))
