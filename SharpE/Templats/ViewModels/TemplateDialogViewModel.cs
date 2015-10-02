using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using SharpE.Definitions.Collection;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Helpers;

namespace SharpE.Templats.ViewModels
{
  public class TemplateDialogViewModel : BaseDialogViewModel
  {
    private Template m_template;
    private readonly IObservableCollection<TemplateParameterViewModel> m_parameters = new AsyncObservableCollection<TemplateParameterViewModel>();
    private readonly ManualCommand m_runCommand;
    private readonly GenericManualCommand<TemplateParameterViewModel> m_browseForFileCommand;
    private bool m_overrideExsistingFiles;
    private readonly IObservableCollection<TemplateCommandViewModel> m_commands = new AsyncObservableCollection<TemplateCommandViewModel>(); 

    public TemplateDialogViewModel()
    {
      m_showCloseButton = false;
      m_runCommand = new ManualCommand(Run);
      m_browseForFileCommand = new GenericManualCommand<TemplateParameterViewModel>(BrowseForFile);
      m_canClose = CloseAutoComplete;
    }

    private bool CloseAutoComplete()
    {
      foreach (TemplateParameterViewModel templateParameterViewModel in m_parameters)
        templateParameterViewModel.ShowAutoComplete = false;
      return true;
    }

    protected override FrameworkElement GenerateView()
    {
      return new TempletDialogView { DataContext = this };
    }

    private void BrowseForFile(TemplateParameterViewModel obj)
    {
      switch (obj.Type)
      {
        case TemplateParameterType.File:
          {
            FileDialog fileDialog = new OpenFileDialog();
            if (File.Exists(obj.Value))
            {
              fileDialog.InitialDirectory = Path.GetDirectoryName(obj.Value);
              fileDialog.ShowHelp = true;
              fileDialog.FileName = obj.Value;
           }
            else 
              fileDialog.InitialDirectory = m_template.TemplatePath;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
              obj.Value = fileDialog.FileName;
            }
          }
          break;
        case TemplateParameterType.Folder:
          {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
              SelectedPath = Directory.Exists(obj.Value) ? obj.Value : m_template.TemplatePath,
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
              obj.Value = folderBrowserDialog.SelectedPath;
          }
          break;
      }
    }

    public void Run()
    {
      foreach (TemplateCommandViewModel templateCommandViewModel in m_commands.Where(n => n.IsSelectedToRun))
      {
        templateCommandViewModel.HasRun = false;
        templateCommandViewModel.Run();
      }
    }

    public Template Template
    {
      get { return m_template; }
      set
      {
        m_template = value;
        m_template.Init();
        Parameters.Clear();
        foreach (TemplateParameter templateParameter in m_template.Parameters)
          Parameters.Add(new TemplateParameterViewModel(templateParameter, m_template));
        m_commands.Clear();
        foreach (TemplateCommand templateCommand in m_template.Commands)
          m_commands.Add(new TemplateCommandViewModel(templateCommand, m_parameters));
        OnPropertyChanged();
      }
    }

    public IObservableCollection<TemplateParameterViewModel> Parameters
    {
      get { return m_parameters; }
    }

    public ManualCommand RunCommand
    {
      get { return m_runCommand; }
    }

    public GenericManualCommand<TemplateParameterViewModel> BrowseForFileCommand
    {
      get { return m_browseForFileCommand; }
    }

    public bool OverrideExsistingFiles
    {
      get { return m_overrideExsistingFiles; }
      set
      {
        if (value.Equals(m_overrideExsistingFiles)) return;
        m_overrideExsistingFiles = value;
        OnPropertyChanged();
      }
    }

    public IObservableCollection<TemplateCommandViewModel> Commands
    {
      get { return m_commands; }
    }
  }
}
