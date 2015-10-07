using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using SharpE.ViewModels;
using SharpE.ViewModels.Layout;
using SharpE.ViewModels.Tree;

namespace SharpE
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      List<string> files = new List<string>();
      StringBuilder errors = new StringBuilder();
      MainViewModel mainViewModel = new MainViewModel();

      foreach (string arg in e.Args)
      {
        if (arg.StartsWith("-"))
        {
          Regex regex = new Regex("-([a-zA-Z]*):?(.*)");
          Match match = regex.Match(arg);
          if (match.Success)
          {
            string parameter = match.Groups[2].ToString();
            switch (match.Groups[1].ToString().ToLower())
            {
              case "folder":
                if (Directory.Exists(parameter))
                {
                  mainViewModel.Path = parameter;
                }
                else
                {
                  errors.AppendLine("Folder does not exist: " + parameter);
                }
                break;
              case "closeall":
                foreach (LayoutElementViewModel layoutElementViewModel in mainViewModel.LayoutManager.LayoutElements)
                {
                  layoutElementViewModel.OpenFiles.Clear();
                  layoutElementViewModel.FileUseOrder.Clear();
                  layoutElementViewModel.SelectedFile = null;
                }
                break;
              case "layout":
                LayoutType layoutType;
                if (Enum.TryParse(parameter, true, out layoutType))
                  mainViewModel.LayoutManager.SelectedLayoutType = layoutType;
                else
                  errors.AppendLine("Unknown layout: " + parameter);
                break;
              default:
                errors.AppendLine("Unknown command: " + arg);
                break;
            }
          }
          else
          {
            errors.AppendLine("Argument with wrong format: " + arg);
          }
        }
        else
        {
          if (File.Exists(arg))
            files.Add(arg);
          else
          {
            errors.Append("Can't find file: " + arg);
          }
        }
      }

      foreach (string file in files)
        mainViewModel.OpenFile(file);

      if (errors.Length > 0)
      {
        FileViewModel fileViewModel = new FileViewModel(errors.ToString(), "Errors");
        mainViewModel.OpenFile(fileViewModel);
      }
      
      mainViewModel.Window.ShowDialog();
      Shutdown();
    }
  }
}
