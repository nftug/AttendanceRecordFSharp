namespace AttendanceRecord.Presentation.Views.HomePage

open type NXUI.Builders
open NXUI.Extensions
open R3

module HomePageView =
    let create (props: HomePageContext) : Avalonia.Controls.Control =
        HomePageContextProvider.provide
            props
            (DockPanel()
                .LastChildFill(true)
                .Children(
                    StackPanel()
                        .DockBottom()
                        .Margin(30.0)
                        .Spacing(25.0)
                        .HorizontalAlignmentStretch()
                        .VerticalAlignmentCenter()
                        .Children(HomeActionsView.create (), StatusView.create ()),
                    ClockView.create ()
                ))
