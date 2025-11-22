namespace AttendanceRecord.Presentation.Views.SettingsPage

open type NXUI.Builders
open NXUI.Extensions
open Material.Icons
open AttendanceRecord.Presentation.Utils
open AttendanceRecord.Presentation.Views.SettingsPage.Context
open AttendanceRecord.Presentation.Views.SettingsPage.Sections
open AttendanceRecord.Presentation.Views.Common.Navigation

module SettingsPageView =
    let create (props: SettingsPageContextProps) : Avalonia.Controls.Control =
        withLifecycle (fun disposables self ->
            let ctx = SettingsPageContext.create props disposables

            match Context.resolve<NavigationContext> self with
            | Some(navCtx, _) -> navCtx.RegisterGuard ctx.ConfirmDiscard |> disposables.Add
            | None -> ()

            ctx
            |> Context.provide (
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
                                    .Spacing(20.0)
                                    .Children(
                                        ThemeSection.create (),
                                        BasicSettingsSection.create (),
                                        WorkEndAlarmSection.create (),
                                        RestStartAlarmSection.create ()
                                    )
                            )
                            .Row(0),
                        SettingsActionsView.create { SaveAppConfig = props.SaveAppConfig }
                        |> _.Row(1)
                    )
            ))
