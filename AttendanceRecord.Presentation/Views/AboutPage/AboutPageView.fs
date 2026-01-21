namespace AttendanceRecord.Presentation.Views.AboutPage

open type NXUI.Builders
open NXUI.Extensions
open System
open System.Reflection
open AttendanceRecord.Presentation.Utils

module AboutPageView =
   let create () : Avalonia.Controls.Control =
      let entryAssembly = Assembly.GetEntryAssembly()
      let version = entryAssembly.GetName().Version.ToString()
      let year = DateTime.Now.Year

      let author =
         entryAssembly.GetCustomAttributes<AssemblyCompanyAttribute>()
         |> Seq.tryHead
         |> Option.map _.Company
         |> Option.defaultValue "Unknown Author"

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
            TextBlock().Text($"Version {version}").FontSize(24.0).HorizontalAlignmentCenter(),
            TextBlock()
               .Text($"Copyright © {year} {author}")
               .FontSize(16.0)
               .Opacity(0.7)
               .Margin(0.0, 30.0, 0.0, 0.0)
               .HorizontalAlignmentCenter()
         )
