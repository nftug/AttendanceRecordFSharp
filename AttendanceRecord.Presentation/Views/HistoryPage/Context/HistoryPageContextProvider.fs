namespace AttendanceRecord.Presentation.Views.HistoryPage.Context

open System
open AttendanceRecord.Presentation.Utils
open R3
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.Dtos.Requests

type WorkRecordStatus =
    { WorkTimeDuration: TimeSpan
      RestTimeDuration: TimeSpan
      OvertimeDuration: TimeSpan }

module WorkRecordStatus =
    let empty () : WorkRecordStatus =
        { WorkTimeDuration = TimeSpan.Zero
          RestTimeDuration = TimeSpan.Zero
          OvertimeDuration = TimeSpan.Zero }

    let fromDetails (details: WorkRecordDetailsDto) : WorkRecordStatus =
        { WorkTimeDuration = details.WorkTime
          RestTimeDuration = details.RestTime
          OvertimeDuration = details.Overtime }

type HistoryPageContext =
    { CurrentMonth: ReactiveProperty<DateTime>
      CurrentDate: ReactiveProperty<DateTime option>
      IsFormDirty: ReactiveProperty<bool>
      MonthlyRecords: ReadOnlyReactiveProperty<WorkRecordListDto>
      Form: ReactiveProperty<WorkRecordSaveRequestDto option>
      CurrentStatus: ReactiveProperty<WorkRecordStatus option> }

module HistoryPageContextProvider =
    let provide
        (value: HistoryPageContext)
        (content: Avalonia.Controls.Control)
        : ContextProvider<HistoryPageContext> =
        Context.provide value content

    let require (control: Avalonia.Controls.Control) : (HistoryPageContext * CompositeDisposable) =
        Context.require<HistoryPageContext> control
