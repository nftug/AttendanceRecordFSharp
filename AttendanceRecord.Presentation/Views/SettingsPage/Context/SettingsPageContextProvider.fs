namespace AttendanceRecord.Presentation.Views.SettingsPage.Context

open AttendanceRecord.Presentation.Utils
open R3

module SettingsPageContextProvider =
    let provide
        (content: Avalonia.Controls.Control)
        (value: SettingsPageContext)
        : Avalonia.Controls.Control =
        Context.provide content value

    let require (control: Avalonia.Controls.Control) : (SettingsPageContext * CompositeDisposable) =
        Context.require<SettingsPageContext> control
