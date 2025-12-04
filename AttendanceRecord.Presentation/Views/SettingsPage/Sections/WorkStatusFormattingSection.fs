namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open R3
open type NXUI.Builders
open FluentAvalonia.UI.Controls
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.Services

[<AutoOpen>]
module private WorkStatusFormattingDialogView =
    open NXUI.Extensions

    let createDialogContent (formatText: ReactiveProperty<string>) =
        withLifecycle (fun _ self ->
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

            self.Loaded.Add(fun _ ->
                // Reset undo stack to prevent large memory usage
                textBox.IsUndoEnabled <- false
                textBox.IsUndoEnabled <- true
                textBox.Focus() |> ignore)

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

[<AutoOpen>]
module private WorkStatusFormattingDialog =
    type FormDialogButton =
        | OkButton
        | CancelButton

    let showDialog
        (ctx: SettingsPageContext)
        (root: Avalonia.Controls.Control)
        (disposables: CompositeDisposable)
        =
        invokeTask disposables (fun ct ->
            task {
                let formatText =
                    R3.property ctx.FormCtx.Form.Value.WorkStatusFormat
                    |> R3.disposeWith disposables

                let defaultValue = formatText.CurrentValue

                let dialog =
                    TaskDialog(
                        Title = getApplicationTitle (),
                        Header = "勤務記録コピーの書式設定",
                        Content = createDialogContent formatText,
                        Buttons =
                            ([ TaskDialogButton("OK", OkButton)
                               TaskDialogButton("キャンセル", CancelButton) ]
                             |> List.toArray),
                        XamlRoot = root
                    )

                dialog.add_Closing (fun _ e ->
                    task {
                        let deferral = e.GetDeferral()

                        match (e.Result :?> FormDialogButton) with
                        | CancelButton when formatText.CurrentValue <> defaultValue ->
                            let! confirm =
                                Dialog.show
                                    { Title = "変更の確認"
                                      Message = "保存されていない変更があります。\n変更を破棄してもよろしいですか？"
                                      Buttons = YesNoButton(Some "破棄", Some "キャンセル") }
                                    (Some ct)

                            e.Cancel <- confirm <> YesResult
                        | _ -> ()

                        deferral.Complete()
                    }
                    |> ignore)

                let! result = dialog.ShowAsync()
                ct.ThrowIfCancellationRequested()

                if result = OkButton then
                    ctx.FormCtx.Form.Value <-
                        { ctx.FormCtx.Form.Value with
                            WorkStatusFormat = formatText.CurrentValue }
            })
        |> ignore

module WorkStatusFormattingSection =
    open NXUI.Extensions

    let create () =
        withLifecycle (fun disposables self ->
            let ctx = Context.require<SettingsPageContext> self |> fst

            let footer =
                Button()
                    .Content(
                        SymbolIconLabel.create
                            { Symbol = Symbol.Edit |> R3.ret
                              Label = "書式を編集..." |> R3.ret
                              Spacing = Some 7.5 |> R3.ret }
                    )
                    .OnClickHandler(fun _ _ -> showDialog ctx self disposables)

            SettingsExpander(
                Header = "勤務記録コピーの書式設定",
                Description = "勤務記録をクリップボードにコピーする際の書式を設定します。",
                IconSource = SymbolIconSource(Symbol = Symbol.Copy),
                Footer = footer
            ))
