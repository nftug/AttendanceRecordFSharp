namespace AttendanceRecord.Presentation.Views.HomePage.Context

open R3
open AttendanceRecord.Application.Dtos.Responses
open AttendanceRecord.Application.UseCases.WorkRecords

type HomePageContext =
    { Status: Observable<WorkStatusDto>
      ToggleWork: ToggleWork
      ToggleRest: ToggleRest }
