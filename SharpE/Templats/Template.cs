using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.MvvmTools.Properties;

namespace SharpE.Templats
{
  public class Template : INotifyPropertyChanged
  {
    private readonly IFileViewModel m_fileViewModel;
    private readonly IList<TemplateParameter> m_parameters = new List<TemplateParameter>();
    private readonly List<TemplateCommand> m_commands = new List<TemplateCommand>();
    private string m_name;
    private readonly TemplateParameter m_targetPathTemplateParameter = new TemplateParameter("targetpath", TemplateParameterType.File);
    private readonly TemplateParameter m_projectPathTemplateParameter = new TemplateParameter("projectpath", TemplateParameterType.File);
    private readonly TemplateParameter m_rootPathTemplateParameter = new TemplateParameter("rootpath", TemplateParameterType.File);
    private readonly string m_templatePath;
    private bool m_overrideExsistingFiles;

    public Template(IFileViewModel fileViewModel)
    {
      m_fileViewModel = fileViewModel;
      fileViewModel.ContentChanged += FileViewModelOnContentChanged;
      m_templatePath = Path.GetDirectoryName(fileViewModel.Path) + "\\";
      JsonNode jsonNode = (JsonNode)JsonHelperFunctions.Parse(FileViewModel.GetContent<string>());
      if (jsonNode == null)
        return;
      Name = jsonNode.GetObjectOrDefault("name", "no name");
    }

    private void FileViewModelOnContentChanged(IFileViewModel fileViewModel)
    {
      JsonNode jsonNode = (JsonNode)JsonHelperFunctions.Parse(FileViewModel.GetContent<string>());
      if (jsonNode == null)
        return;
      Name = jsonNode.GetObjectOrDefault("name", "no name");
    }

    public void Init()
    {
      m_parameters.Clear();
      m_commands.Clear();
      JsonNode jsonNode = (JsonNode)JsonHelperFunctions.Parse(FileViewModel.GetContent<string>());
      if (jsonNode == null)
        return;
      Name = jsonNode.GetObjectOrDefault("name", "no name");
      m_parameters.Add(m_targetPathTemplateParameter);
      m_parameters.Add(m_projectPathTemplateParameter);
      m_parameters.Add(m_rootPathTemplateParameter);
      m_parameters.Add(new TemplateParameter("templatepath", TemplateParameterType.File) { Value = TemplatePath });
      JsonArray parameterDefinitions = jsonNode.GetObjectOrDefault<JsonArray>("parameters", null);
      if (parameterDefinitions != null)
      {
        foreach (JsonNode parameterDefinition in parameterDefinitions)
          m_parameters.Add(new TemplateParameter(parameterDefinition, this));
      }
      JsonArray commandDefinitions = jsonNode.GetObjectOrDefault<JsonArray>("commands", null);
      if (commandDefinitions != null)
      {
        foreach (object commandDefinition in commandDefinitions)
        {
          if (commandDefinition is JsonNode)
            Commands.Add(new TemplateCommand((JsonNode)commandDefinition, this));
        }
      }
    }

    public bool AddParameters(ref string orignal, List<string> errors, List<TemplateParameter> usedParameters, bool isForJson = false, char? pathSeperator = '\\')
    {
      string[] parts = orignal.Split('+');
      bool retval = true;
      orignal = "";
      foreach (string part in parts)
      {
        Regex scheamRegex = new Regex(@"^{([\w\s]+)>*([\w\s]*)}$", RegexOptions.IgnoreCase);
        Match match = scheamRegex.Match(part.Trim());
        if (match.Success)
        {
          string parameter = match.Groups[1].ToString();
          TemplateParameter templateParameter =
            Parameters.FirstOrDefault(n => n.Name == parameter);
          if (templateParameter != null)
          {
            if (isForJson)
            {
              if (templateParameter.Type.IsPath())
              {
                string value = templateParameter.GetJsonValue(pathSeperator);
                if (match.Groups.Count > 2 && !string.IsNullOrEmpty(match.Groups[2].ToString()))
                {
                  TemplateParameter refTemplateParameter =
                    Parameters.FirstOrDefault(n => n.Name == match.Groups[2].ToString());
                  if (refTemplateParameter != null)
                  {
                    string path = Path.GetDirectoryName(refTemplateParameter.Value);
                    if (path != null && value.Length > path.Length + 1)
                      value = value.Substring(path.Length + 1);
                  }
                  else
                  {
                    if (errors != null)
                      errors.Add("Parameter " + match.Groups[2] + " missing.");
                    retval = false;
                  }
                }
                orignal += value;
              }
              else
              {
                orignal += templateParameter.GetJsonValue(pathSeperator);
              }
            }
            else
              orignal += templateParameter.Value;
            if (usedParameters != null && !usedParameters.Contains(templateParameter))
              usedParameters.Add(templateParameter);
          }
          else
          {
            if (errors != null)
              errors.Add("Parameter " + match.Groups[1] + " missing.");
            retval = false;
          }
        }
        else
          orignal += part;
      }
      return retval;
    }


    public IList<TemplateParameter> Parameters
    {
      get { return m_parameters; }
    }

    public string Name
    {
      get { return m_name; }
      set
      {
        if (value == m_name) return;
        m_name = value;
        OnPropertyChanged();
      }
    }

    public string TargetPath
    {
      get { return m_targetPathTemplateParameter.Value; }
      set { m_targetPathTemplateParameter.Value = value + "\\"; }
    }

    public string ProjectPath
    {
      get { return m_projectPathTemplateParameter.Value; }
      set { m_projectPathTemplateParameter.Value = value + "\\"; }
    }

    public string RootPath
    {
      get { return m_rootPathTemplateParameter.Value; }
      set { m_rootPathTemplateParameter.Value = value + "\\"; }
    }

    public bool OverrideExsistingFiles
    {
      get { return m_overrideExsistingFiles; }
    }

    public string TemplatePath
    {
      get { return m_templatePath; }
    }

    public List<TemplateCommand> Commands
    {
      get { return m_commands; }
    }

    public IFileViewModel FileViewModel
    {
      get { return m_fileViewModel; }
    }

    public void Run(bool overrideExsistingFiles)
    {
      m_overrideExsistingFiles = overrideExsistingFiles;
      foreach (TemplateCommand templateCommand in Commands)
        templateCommand.Execute();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
