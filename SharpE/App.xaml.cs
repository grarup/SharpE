using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using SharpE.Json.Data;
using SharpE.Json.Schemas;
using SharpE.MvvmTools.Helpers;
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
      string outputPath = null;
      bool silent = false;
      MainViewModel mainViewModel = new MainViewModel();

      foreach (string arg in e.Args)
      {
        if (arg.StartsWith("-"))
        {
          Regex regex = new Regex("-([a-zA-Z]*):?([^,]*),?([^,]*),?([^,]*)");
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
              case "validate":
                if (File.Exists(parameter))
                {
                  JsonException jsonException;
                  string text = File.ReadAllText(parameter);
                  JsonObject jsonObject = JsonHelperFunctions.Parse(text, out jsonException);
                  if (jsonException != null)
                  {
                    errors.AppendLine("json error: " + jsonException.Message);
                    Environment.ExitCode = 1;
                    break;
                  }
                  Regex scheamRegex = new Regex(@"""\$schema""\s*\:\s*""(.*)""", RegexOptions.IgnoreCase);
                  Match schemaMatch = scheamRegex.Match(text);
                  Schema schema = schemaMatch.Success ? mainViewModel.SchemaManager.GetSchema(schemaMatch.Groups[1].ToString()) : null;
                  if (schema == null)
                  {
                    errors.AppendLine("Sschema not found: " + arg);
                    Environment.ExitCode = 2;
                    break;
                  }
                  Dictionary<int, List<ValidationError>> validationErrors = new Dictionary<int, List<ValidationError>>();
                  schema.Validate(jsonObject, validationErrors, Path.GetDirectoryName(parameter));
                  if (validationErrors.Count > 0)
                  {
                    Environment.ExitCode = 1;
                    foreach (KeyValuePair<int, List<ValidationError>> validationError in validationErrors)
                    {
                      foreach (ValidationError error in validationError.Value)
                      {
                        errors.AppendLine(validationError.Key + ": " + error.Message);
                      }
                    }
                  }
                }
                else
                {
                  errors.AppendLine("File not found: " + parameter);
                  Environment.ExitCode = 2;
                }
                break;
              case "silent":
                silent = true;
                break;
              case "output":
                outputPath = parameter;
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


      
      if (!silent)
      {
        if (outputPath != null)
        {
          File.WriteAllText(outputPath, errors.ToString());
        }
        else
        {
          if (errors.Length > 0)
          {
            FileViewModel fileViewModel = new FileViewModel(errors.ToString(), "Errors");
            mainViewModel.OpenFile(fileViewModel);
          }

          mainViewModel.Window.ShowDialog();
        }
      }

      Shutdown();
    }
  }
}
