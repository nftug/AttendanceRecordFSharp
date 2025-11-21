namespace AttendanceRecord.Presentation.Views.HistoryPage.Context

open AttendanceRecord.Presentation.Utils
open R3

module HistoryPageContextProvider =
    let provide
        (content: Avalonia.Controls.Control)
        (value: HistoryPageContext)
        : Avalonia.Controls.Control =
        Context.provide content value

    let require (control: Avalonia.Controls.Control) : (HistoryPageContext * CompositeDisposable) =
        Context.require<HistoryPageContext> control
