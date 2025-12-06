namespace AttendanceRecord.Presentation.Views.SettingsPage.Sections

open R3
open type NXUI.Builders
open NXUI.Extensions
open FluentAvalonia.UI.Controls
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Requests
open AttendanceRecord.Application.Services

[<AutoOpen>]
module private WorkStatusFormattingSectionContent =
   let createContent (isVisible: Observable<bool>) =
      withLifecycle (fun disposables self ->
         let ctx, _ = Context.require<SettingsPageContext> self
         let mutable textBoxRef: Avalonia.Controls.TextBox | null = null

         self.Loaded.Add(fun _ ->
            // Clear undo stack on load to prevent from undoing to empty state
            textBoxRef.IsUndoEnabled <- false
            textBoxRef.IsUndoEnabled <- true
            textBoxRef.Focus() |> ignore)

         isVisible
         |> R3.filter id
         |> R3.subscribe (fun _ -> textBoxRef.Focus() |> ignore)
         |> disposables.Add

         let insertSnippet (snippet: string) =
            let caret = textBoxRef.CaretIndex
            let before = textBoxRef.Text.Substring(0, caret)
            let after = textBoxRef.Text.Substring caret
            textBoxRef.Text <- before + snippet + after
            textBoxRef.CaretIndex <- caret + snippet.Length
            textBoxRef.Focus() |> ignore

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

                  [ Hours; Minutes; Seconds ]
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
            .Width(520.0)
            .Margin(0.0, 5.0)
            .HorizontalAlignmentLeft()
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
               TextBox(&textBoxRef)
                  .Text(ctx.FormCtx.Form |> R3.map _.WorkStatusFormat |> asBinding)
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
            ))

module WorkStatusFormattingSection =
   let create () =
      let expander =
         SettingsExpander(
            Header = "勤務記録コピーの書式設定",
            Description = "勤務記録をクリップボードにコピーする際の書式を設定します。",
            IconSource = SymbolIconSource(Symbol = Symbol.Copy)
         )

      let isVisible = R3.everyValueChanged expander _.IsExpanded

      expander.Items.Add(SettingsExpanderItem(Content = createContent isVisible))
      |> ignore

      expander
