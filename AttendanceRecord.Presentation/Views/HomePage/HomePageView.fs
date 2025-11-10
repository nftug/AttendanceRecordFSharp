namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open AttendanceRecord.Presentation.Utils
open R3
open System.Threading.Tasks
open System.Threading

open AttendanceRecord.Application.Dtos.Responses

type HomePageViewProps =
    { Status: Observable<CurrentStatusDto>
      OnToggleWork: unit -> CancellationToken -> Task<Result<CurrentStatusDto, string>>
      OnToggleRest: unit -> CancellationToken -> Task<Result<CurrentStatusDto, string>> }

module HomePageView =
    let create (props: HomePageViewProps) : Avalonia.Controls.Control =
        withReactive (fun _ _ ->
            DockPanel()
                .LastChildFill(true)
                .Children(
                    StackPanel()
                        .DockBottom()
                        .Margin(30.0)
                        .Spacing(25.0)
                        .HorizontalAlignmentStretch()
                        .VerticalAlignmentCenter()
                        .Children(
                            HomeActionsView.create
                                { Status = props.Status
                                  OnToggleWork = props.OnToggleWork
                                  OnToggleRest = props.OnToggleRest },
                            StatusView.create { Status = props.Status }
                        ),
                    ClockView.create { Status = props.Status }
                ))
