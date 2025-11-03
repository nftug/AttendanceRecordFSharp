namespace AttendanceRecord.Presentation.Common.Components

open DialogHostAvalonia
open System.Threading.Tasks
open System.Windows.Input
open Avalonia
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Media
open Avalonia.Controls
open Avalonia.Layout

type ConfirmDialogViewProps = { Title: string; Message: string }

module ConfirmDialogView =
    let private create (props: ConfirmDialogViewProps) (closeCommand: ICommand) : Border =
        Component(fun ctx ->
            StackPanel.create
                [ StackPanel.margin 20.0
                  StackPanel.spacing 20.0
                  StackPanel.minWidth 400.0
                  StackPanel.maxWidth 500.0
                  StackPanel.children
                      [ CjkTextBlock.create
                            [ TextBlock.text props.Title
                              TextBlock.fontSize 20.0
                              TextBlock.fontWeight FontWeight.Bold
                              TextBlock.margin (Thickness(0.0, 0.0, 0.0, 10.0)) ]
                        CjkTextBlock.create
                            [ TextBlock.text props.Message
                              TextBlock.fontSize 16.0
                              TextBlock.textWrapping TextWrapping.Wrap
                              TextBlock.margin (Thickness(0.0, 0.0, 0.0, 20.0)) ]
                        StackPanel.create
                            [ StackPanel.orientation Orientation.Horizontal
                              StackPanel.horizontalAlignment HorizontalAlignment.Right
                              StackPanel.margin (Thickness(0.0, 10.0, 0.0, 0.0))
                              StackPanel.spacing 20.0
                              StackPanel.children
                                  [ Button.create
                                        [ Button.content (CjkTextBlock.create [ TextBlock.text "いいえ" ])
                                          Button.width 80.0
                                          Button.onClick (fun _ -> closeCommand.Execute(box false)) ]
                                    Button.create
                                        [ Button.content (CjkTextBlock.create [ TextBlock.text "はい" ])
                                          Button.width 80.0
                                          Button.onClick (fun _ -> closeCommand.Execute(box true)) ] ] ] ] ])

    let show (props: ConfirmDialogViewProps) : Task<bool> =
        task {
            let dialogHost = tryGetControl<DialogHost> ()

            match dialogHost with
            | Some host ->
                let content = create props host.CloseDialogCommand
                let! result = DialogHost.Show content
                return result :?> bool
            | None ->
                invalidOp "DialogHost is not found."
                return false
        }
