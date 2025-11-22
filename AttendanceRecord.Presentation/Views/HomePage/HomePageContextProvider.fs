namespace AttendanceRecord.Presentation.Views.HomePage

open AttendanceRecord.Presentation.Utils
open R3
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.UseCases.WorkRecords

type HomePageContext =
    { Status: Observable<CurrentStatusDto>
      ToggleWork: ToggleWork
      ToggleRest: ToggleRest }

module HomePageContextProvider =
    let provide
        (content: Avalonia.Controls.Control)
        (value: HomePageContext)
        : Avalonia.Controls.Control =
        Context.provide content value

    let require (control: Avalonia.Controls.Control) : (HomePageContext * CompositeDisposable) =
        Context.require<HomePageContext> control
