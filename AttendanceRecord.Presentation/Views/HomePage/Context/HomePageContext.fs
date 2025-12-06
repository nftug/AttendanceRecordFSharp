namespace AttendanceRecord.Presentation.Views.HomePage.Context

open R3
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.UseCases.WorkRecords

type HomePageContext =
   { Status: ReadOnlyReactiveProperty<WorkStatusDto>
     AppConfig: Observable<AppConfigDto>
     ToggleWork: ToggleWork
     ToggleRest: ToggleRest }
