namespace AttendanceRecord.Presentation.Views.HistoryPage.Context

open AttendanceRecord.Presentation.Utils
open R3

module HistoryPageContextProvider =
    let provide
        (value: HistoryPageContext)
        (content: Avalonia.Controls.Control)
        : ContextProvider<HistoryPageContext> =
        Context.provide value content

    let require (control: Avalonia.Controls.Control) : (HistoryPageContext * CompositeDisposable) =
        Context.require<HistoryPageContext> control
