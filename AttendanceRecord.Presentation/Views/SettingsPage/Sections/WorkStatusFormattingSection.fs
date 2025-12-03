namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open NXUI.Extensions
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
    let createDialogContent (ctx: SettingsPageContext) =
        withLifecycle (fun _ self ->
            let formatText = ctx.FormCtx.Form |> R3.map _.WorkStatusFormat

            let textBox =
                TextBox()
                    .Text(formatText |> asBinding)
                    .OnTextChangedHandler(fun ctl _ ->
                        ctx.FormCtx.Form.Value <-
                            { ctx.FormCtx.Form.Value with
                                WorkStatusFormat = ctl.Text })
                    .AcceptsReturn(true)
                    .TextWrappingWrap()
                    .VerticalScrollBarVisibilityAuto()
                    .Height(130.0)
                    .Padding(12.0)
                    .LineHeight(25.0)

            self.Loaded.Add(fun _ -> textBox.Focus() |> ignore)

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

module WorkStatusFormattingSection =
    let create () =
        withLifecycle (fun _ self ->
            let ctx = Context.require<SettingsPageContext> self |> fst

            let showDialog () =
                let dialog =
                    ContentDialog(
                        Title = "勤務記録コピーの書式設定",
                        Content = createDialogContent ctx,
                        CloseButtonText = "閉じる"
                    )

                dialog.ShowAsync() |> ignore

            let footer =
                Button()
                    .Content(
                        SymbolIconLabel.create
                            { Symbol = Symbol.Edit |> R3.ret
                              Label = "書式を編集..." |> R3.ret
                              Spacing = Some 7.5 |> R3.ret }
                    )
                    .OnClickHandler(fun _ _ -> showDialog ())

            SettingsExpander(
                Header = "勤務記録コピーの書式設定",
                Description = "勤務記録をクリップボードにコピーする際の書式を設定します。",
                IconSource = SymbolIconSource(Symbol = Symbol.Copy),
                Footer = footer
            ))
