namespace AttendanceRecord.Presentation.Views.AboutPage

open type NXUI.Builders
open NXUI.Extensions
open System.Reflection
open AttendanceRecord.Presentation.Utils

module AboutPageView =
   let create () : Avalonia.Controls.Control =
      let version = Assembly.GetEntryAssembly().GetName().Version.ToString()

      StackPanel()
         .HorizontalAlignmentCenter()
         .VerticalAlignmentCenter()
         .Children(
            TextBlock()
               .Text(getApplicationTitle ())
               .FontSize(32.0)
               .FontWeightBold()
               .HorizontalAlignmentCenter()
               .Margin(0.0, 0.0, 0.0, 30.0),
            TextBlock().Text($"Version {version}").FontSize(24.0).HorizontalAlignmentCenter()
         )
