namespace AttendanceRecord.Presentation.Views.HistoryPage

open System
open R3
open AttendanceRecord.Shared
open AttendanceRecord.Application.Dtos.Responses

type HistoryPageHooks =
    { CurrentMonth: ReactiveProperty<DateTime>
      CurrentDate: ReactiveProperty<DateTime option>
      MonthlyRecords: ReadOnlyReactiveProperty<WorkRecordListDto>
      SelectedRecord: ReadOnlyReactiveProperty<WorkRecordDetailsDto option> }

[<AutoOpen>]
module HistoryPageHooks =
    let useHistoryPageHooks
        (ctx: HistoryPageContext)
        (disposables: CompositeDisposable)
        : HistoryPageHooks =
        let monthlyRecords =
            ctx.MonthlyRecords |> R3.readonly None |> R3.disposeWith disposables

        let selectedRecord =
            ctx.SelectedRecord |> R3.readonly None |> R3.disposeWith disposables

        { CurrentMonth = ctx.CurrentMonth
          CurrentDate = ctx.CurrentDate
          MonthlyRecords = monthlyRecords
          SelectedRecord = selectedRecord }
