namespace AttendanceRecord.Presentation.Views.HistoryPage.Edit

open System
open System.Threading
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
module private WorkRecordEditViewHelpers =
    let handleClickSave
        (handle: WorkRecordSaveRequestDto -> CancellationToken -> Tasks.Task<Result<Guid, string>>)
        (ctx: HistoryPageContext)
        (disposables: R3.CompositeDisposable)
        : unit =
        invokeTask disposables (fun ct ->
            task {
                match ctx.Form.Value with
                | Some request ->
                    let! result = handle request ct

                    match result with
                    | Ok id ->
                        Notification.show
                            { Title = "保存完了"
                              Message = "勤務記録を保存しました。"
                              NotificationType = NotificationType.Success }

                        ctx.ReloadAfterSave(Some id)
                    | Error e ->
                        Notification.show
                            { Title = "保存エラー"
                              Message = $"勤務記録の保存に失敗しました: {e}"
                              NotificationType = NotificationType.Error }
                | None -> ()
            })
        |> ignore

    let handleClickDelete
        (handle: Guid -> CancellationToken -> Tasks.Task<Result<unit, string>>)
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
                        let! result = handle r.Id.Value ct

                        match result with
                        | Ok _ ->
                            Notification.show
                                { Title = "削除完了"
                                  Message = "勤務記録を削除しました。"
                                  NotificationType = NotificationType.Success }

                            ctx.CurrentDate.Value <- None
                            ctx.ReloadAfterSave None
                        | Error e ->
                            Notification.show
                                { Title = "削除エラー"
                                  Message = $"勤務記録の削除に失敗しました: {e}"
                                  NotificationType = NotificationType.Error }
                | _ -> ()
            })
        |> ignore

module WorkRecordEditView =
    open NXUI.Extensions
    open type NXUI.Builders
    open Avalonia.Media
    open Material.Icons

    let create (props: WorkRecordEditViewProps) : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<HistoryPageContext> self
            let saveMutation = useMutation disposables props.SaveWorkRecord.Handle
            let deleteMutation = useMutation disposables props.DeleteWorkRecord.Handle

            let isFormDirty =
                R3.combineLatest2 ctx.Form ctx.DefaultForm |> R3.map (fun (f, d) -> f <> d)

            let saveButtonEnabled =
                R3.combineLatest2 saveMutation.IsPending isFormDirty
                |> R3.map (fun (isSaving, dirty) -> not isSaving && dirty)

            let deleteButtonEnabled =
                R3.combineLatest2 ctx.CurrentRecord deleteMutation.IsPending
                |> R3.map (fun (rOpt, isDeleting) -> rOpt.IsSome && not isDeleting)

            let actionButtons =
                StackPanel()
                    .OrientationHorizontal()
                    .Spacing(10.0)
                    .Children(
                        Button()
                            .Content(MaterialIconLabel.create MaterialIconKind.Refresh "リセット")
                            .OnClickHandler(fun _ _ -> ctx.Form.Value <- ctx.DefaultForm.Value)
                            .Width(100.0)
                            .Height(35.0)
                            .IsEnabled(isFormDirty |> asBinding),
                        Button()
                            .Content(MaterialIconLabel.create MaterialIconKind.ContentSave "保存")
                            .OnClickHandler(fun _ _ ->
                                handleClickSave saveMutation.MutateTask ctx disposables)
                            .Width(100.0)
                            .Height(35.0)
                            .IsEnabled(saveButtonEnabled |> asBinding)
                        |> Colors.setAccentColorBackground
                    )

            let deleteButton =
                Button()
                    .Content(MaterialIconLabel.create MaterialIconKind.Delete "削除")
                    .OnClickHandler(fun _ _ ->
                        handleClickDelete deleteMutation.MutateTask ctx disposables)
                    .Width(100.0)
                    .Height(35.0)
                    .IsEnabled(deleteButtonEnabled |> asBinding)
                    .Background(Brushes.DarkRed)
                    .Foreground(Brushes.White)

            Grid()
                .RowDefinitions("*,Auto")
                .Margin(20.0)
                .RowSpacing(20.0)
                .Children(
                    Grid()
                        .RowDefinitions("Auto,*")
                        .RowSpacing(20.0)
                        .Children(
                            StackPanel()
                                .Spacing(20.0)
                                .Children(
                                    TextBlock()
                                        .Text(
                                            ctx.Form
                                            |> R3.map (function
                                                | Some f -> f.StartedAt
                                                | None -> DateTime.MinValue)
                                            |> R3.map _.ToString("yyyy/MM/dd (ddd)")
                                            |> asBinding
                                        )
                                        .FontSize(28.0)
                                        .FontWeightBold(),
                                    WorkStatusSummarySection.create (),
                                    WorkTimeSection.create ()
                                )
                                .Row(0),
                            RestTimeSection.create () |> _.Row(1)
                        )
                        .Row(0),
                    Grid()
                        .ColumnDefinitions("Auto,*,Auto")
                        .Children(deleteButton.Column(0), actionButtons.Column(2))
                        .Row(1)
                ))
