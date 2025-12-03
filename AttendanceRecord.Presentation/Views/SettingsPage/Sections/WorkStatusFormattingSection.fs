namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open R3
open FluentAvalonia.UI.Controls
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.Services

module private WorkStatusFormattingDialogView =
    open NXUI.Extensions
    open type NXUI.Builders

    let createDialogContent (ctx: SettingsPageContext) (formatText: ReactiveProperty<string>) =
        withLifecycle (fun disposables _ ->
            let textBox =
                TextBox()
                    .Text(formatText |> asBinding)
                    .OnTextChangedHandler(fun ctl _ -> formatText.Value <- ctl.Text)
                    .AcceptsReturn(true)
                    .TextWrappingWrap()
                    .VerticalScrollBarVisibilityAuto()
                    .Height(130.0)
                    .Padding(12.0)
                    .LineHeight(25.0)

            nextTick (fun () -> textBox.Focus() |> ignore)

            ctx.FormCtx.Form
            |> R3.map _.WorkStatusFormat
            |> R3.subscribe (fun format ->
                textBox.IsUndoEnabled <- false
                formatText.Value <- format
                textBox.IsUndoEnabled <- true)
            |> disposables.Add

            let insertSnippet (snippet: string) =
                let caret = textBox.CaretIndex
                let before = textBox.Text.Substring(0, caret)
                let after = textBox.Text.Substring caret
                textBox.Text <- before + snippet + after
                textBox.CaretIndex <- caret + snippet.Length
                textBox.Focus() |> ignore

            let createSnippetButton (timeType: TimeType) =
                DropDownButton()
                    .Content(
                        match timeType with
                        | WorkTime -> "勤務時間"
                        | RestTime -> "休憩時間"
                        | Overtime -> "残業時間 "
                        | OvertimeMonthly -> "残業時間 (月)"
                    )
                    .Flyout(
                        let flyout = MenuFlyout()

                        [ Hours; Minutes ]
                        |> List.map (fun formatItem ->
                            MenuItem()
                                .Header(
                                    match formatItem with
                                    | Hours -> "時間を挿入"
                                    | Minutes -> "分を挿入"
                                    | Seconds -> "秒を挿入"
                                )
                                .OnClickHandler(fun _ _ ->
                                    TimeFormat(timeType, formatItem)
                                    |> WorkStatusFormatter.toFormatString
                                    |> insertSnippet))
                        |> List.iter (flyout.Items.Add >> ignore)

                        flyout
                    )
                    .Focusable(false)

            DockPanel()
                .LastChildFill(true)
                .Children(
                    StackPanel()
                        .DockTop()
                        .OrientationHorizontal()
                        .Margin(0.0, 0.0, 0.0, 10.0)
                        .Spacing(5.0)
                        .Children(
                            [ WorkTime; RestTime; Overtime; OvertimeMonthly ]
                            |> List.map createSnippetButton
                            |> toChildren
                        ),
                    textBox
                ))

module private WorkStatusFormattingDialog =
    let show (ctx: SettingsPageContext) (disposables: CompositeDisposable) =
        invokeTask disposables (fun ct ->
            task {
                let formatText = R3.property "" |> R3.disposeWith disposables

                let content = WorkStatusFormattingDialogView.createDialogContent ctx formatText
                let dialog = ContentDialog()
                dialog.Title <- "勤務記録コピーの書式設定"
                dialog.Content <- content
                dialog.CloseButtonText <- "閉じる"

                dialog.PrimaryButtonCommand <-
                    R3.command ()
                    |> R3.withSubscribe disposables (fun _ -> printfn "Resetting format")

                let! _ = dialog.ShowAsync()
                ct.ThrowIfCancellationRequested()

                ctx.FormCtx.Form.Value <-
                    { ctx.FormCtx.Form.Value with
                        WorkStatusFormat = formatText.Value }
            })

module WorkStatusFormattingSection =
    open NXUI.Extensions
    open type NXUI.Builders
    open AttendanceRecord.Presentation.Views.Common

    let create () =
        withLifecycle (fun disposables self ->
            let ctx, _ = Context.require<SettingsPageContext> self

            let footer =
                Button()
                    .Content(
                        SymbolIconLabel.create
                            { Symbol = Symbol.Edit |> R3.ret
                              Label = "書式を編集..." |> R3.ret
                              Spacing = Some 7.5 |> R3.ret }
                    )
                    .OnClickHandler(fun _ _ ->
                        WorkStatusFormattingDialog.show ctx disposables |> ignore)

            SettingsExpander(
                Header = "勤務記録コピーの書式設定",
                Description = "勤務記録をクリップボードにコピーする際の書式を設定します。",
                IconSource = SymbolIconSource(Symbol = Symbol.Copy),
                Footer = footer
            ))
