namespace AttendanceRecord.Presentation.Views.HomePage

open AttendanceRecord.Presentation.Utils
open R3
open System.Threading.Tasks
open System.Threading
open AttendanceRecord.Application.Dtos.Responses

type HomePageContext =
    { Status: Observable<CurrentStatusDto>
      OnToggleWork: unit -> CancellationToken -> Task<Result<CurrentStatusDto, string>>
      OnToggleRest: unit -> CancellationToken -> Task<Result<CurrentStatusDto, string>> }

module HomePageContextProvider =
    let provide
        (value: HomePageContext)
        (content: Avalonia.Controls.Control)
        : ContextProvider<HomePageContext> =
        Context.provide value content

    let require (control: Avalonia.Controls.Control) : (HomePageContext * CompositeDisposable) =
        Context.require<HomePageContext> control
