namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open FluentAvalonia.UI.Controls
open R3
open System
open AttendanceRecord.Shared
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Presentation.Views.HomePage.Context
open AttendanceRecord.Presentation.Views.Common
open AttendanceRecord.Presentation.Views.Common.Context
open AttendanceRecord.Application.Services

module StatusView =
   let private createSummaryInfoRow (label: string) (duration: Observable<TimeSpan>) =
      let durationText = duration |> R3.map TimeSpan.formatDuration

      StackPanel()
         .OrientationHorizontal()
         .Spacing(5.0)
         .Children(
            TextBlock().Text(label).FontWeightBold().FontSize(16.0).Width(150.0),
            TextBlock().Text(durationText |> asBinding).FontSize(16.0).VerticalAlignmentCenter()
         )

   let create () : Avalonia.Controls.Control =
      withLifecycle (fun disposables self ->
         let ctx, _ = Context.require<HomePageContext> self
         let themeCtx = Context.require<ThemeContext> self |> fst
         let appConfig = ctx.AppConfig |> R3.readonly None |> R3.disposeWith disposables

         let handleClickCopyButton () =
            let status = ctx.Status.CurrentValue
            let config = appConfig.CurrentValue
            let formattedText = WorkStatusFormatter.format config.WorkStatusFormat status

            getMainWindow () |> _.Clipboard.SetTextAsync(formattedText) |> ignore

            Notification.show
               { Title = "勤務記録のコピー"
                 Message = "勤務記録をクリップボードにコピーしました。"
                 NotificationType = InformationNotification }

         (ControlBorder.create themeCtx)
            .Padding(25.0)
            .Height(250.0)
            .Child(
               Panel()
                  .Children(
                     StackPanel()
                        .Spacing(8.0)
                        .VerticalAlignmentCenter()
                        .Children(
                           createSummaryInfoRow
                              "勤務時間"
                              (ctx.Status |> R3.map _.Summary.TotalWorkTime),
                           createSummaryInfoRow
                              "休憩時間"
                              (ctx.Status |> R3.map _.Summary.TotalRestTime),
                           createSummaryInfoRow "今日の残業時間" (ctx.Status |> R3.map _.Summary.Overtime),
                           createSummaryInfoRow "今月の残業時間" (ctx.Status |> R3.map _.OvertimeMonthly)
                        ),
                     Button()
                        .Content(SymbolIcon.create (Symbol.Copy |> R3.ret))
                        .FontSize(28.0)
                        .Padding(10.0)
                        .VerticalAlignmentBottom()
                        .HorizontalAlignmentRight()
                        .Tip("勤務記録をクリップボードにコピー")
                        .OnClickHandler(fun _ _ -> handleClickCopyButton ())
                  )
            ))
