namespace AttendanceRecord.Presentation.Views.SettingsPage

open type NXUI.Builders
open NXUI.Extensions
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Presentation.Views.SettingsPage.Sections
open AttendanceRecord.Presentation.Views.Common.Navigation

module SettingsPageView =
    let create (props: SettingsPageContextProps) : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx = SettingsPageContext.create props disposables

            Context.resolve<NavigationContext> self
            |> Option.iter (fun (navCtx, _) ->
                navCtx.RegisterGuard ctx.ConfirmDiscard |> disposables.Add)

            Grid()
                .RowDefinitions("*,Auto")
                .Margin(20.0)
                .RowSpacing(20.0)
                .Children(
                    ScrollViewer()
                        .HorizontalScrollBarVisibilityDisabled()
                        .VerticalScrollBarVisibilityAuto()
                        .Content(
                            StackPanel()
                                .HorizontalAlignmentStretch()
                                .Spacing(15.0)
                                .Children(
                                    ThemeSection.create (),
                                    BasicSettingsSection.create (),
                                    WorkEndAlarmSection.create (),
                                    RestStartAlarmSection.create (),
                                    WorkStatusFormattingSection.create ()
                                )
                        )
                        .Row(0),
                    SettingsActionsView.create () |> _.Row(1)
                )
            |> Context.provide ctx)
