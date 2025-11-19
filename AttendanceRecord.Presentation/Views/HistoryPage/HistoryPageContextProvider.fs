namespace AttendanceRecord.Presentation.Views.HistoryPage

open System
open System.Threading
open System.Threading.Tasks
open AttendanceRecord.Presentation.Utils
open R3
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Requests

type HistoryPageContext =
    { CurrentMonth: ReactiveProperty<DateTime>
      CurrentDate: ReactiveProperty<DateTime option>
      MonthlyRecords: Observable<WorkRecordListDto>
      SelectedRecord: Observable<WorkRecordDetailsDto option>
      OnSaveRecord: WorkRecordSaveRequestDto -> CancellationToken -> Task<Result<unit, string>>
      OnDeleteRecord: Guid -> CancellationToken -> Task<Result<unit, string>> }

module HistoryPageContextProvider =
    let provide
        (value: HistoryPageContext)
        (content: Avalonia.Controls.Control)
        : ContextProvider<HistoryPageContext> =
        Context.provide value content

    let require (control: Avalonia.Controls.Control) : (HistoryPageContext * CompositeDisposable) =
        Context.require<HistoryPageContext> control
