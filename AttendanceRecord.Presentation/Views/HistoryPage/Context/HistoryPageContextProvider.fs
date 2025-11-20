namespace AttendanceRecord.Presentation.Views.HistoryPage.Context

open System
open AttendanceRecord.Presentation.Utils
open R3
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Requests

type HistoryPageContext =
    { CurrentMonth: ReactiveProperty<DateTime>
      CurrentDate: ReactiveProperty<DateTime option>
      IsFormDirty: ReactiveProperty<bool>
      MonthlyRecords: ReadOnlyReactiveProperty<WorkRecordListDto>
      Form: ReactiveProperty<WorkRecordSaveRequestDto option>
      CurrentRecord: ReadOnlyReactiveProperty<WorkRecordDetailsDto option>
      ReloadAfterSave: Guid option -> unit }

module HistoryPageContextProvider =
    let provide
        (value: HistoryPageContext)
        (content: Avalonia.Controls.Control)
        : ContextProvider<HistoryPageContext> =
        Context.provide value content

    let require (control: Avalonia.Controls.Control) : (HistoryPageContext * CompositeDisposable) =
        Context.require<HistoryPageContext> control
