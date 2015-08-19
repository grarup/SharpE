using System.Linq;
using System.Windows;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Helpers;
using SharpE.ViewModels.Tree;
using SharpE.Views.Dialogs;

namespace SharpE.ViewModels.Dialogs
{
  class ReloadFilesDialogViewModel : BaseDialogViewModel
  {
    private readonly IObservableCollection<ReloadFileViewModel> m_files = new AsyncObservableCollection<ReloadFileViewModel>();
    private readonly ManualCommand m_ignorAllCommand;
    private readonly ManualCommand m_reloadAllCommand;
    private readonly ManualCommand m_reloadSelectedCommand;
    private readonly ManualCommand m_toggleCommand;
    private ReloadFileViewModel m_selectedFile;

    public ReloadFilesDialogViewModel()
    {
      m_showCloseButton = false;
      m_ignorAllCommand = new ManualCommand(IgnoreAll);
      m_reloadAllCommand = new ManualCommand(ReloadAll);
      m_toggleCommand = new ManualCommand(() => { if (m_selectedFile != null) m_selectedFile.IsChecked = !m_selectedFile.IsChecked;});
      m_reloadSelectedCommand = new ManualCommand(ReloadSelected);
    }

    protected override FrameworkElement GenerateView()
    {
      return new ReloadFilesView {DataContext = this};
    }

    private  void ReloadAll()
    {
      lock (m_files)
      {
        foreach (ReloadFileViewModel reloadFileViewModel in m_files)
          ((FileViewModel) reloadFileViewModel.File).Reload();
        m_files.Clear();
        IsShown = false;
      }
    }

    private  void ReloadSelected()
    {
      lock (m_files)
      {
        foreach (ReloadFileViewModel reloadFileViewModel in m_files)
        {
          if (reloadFileViewModel.IsChecked)
            ((FileViewModel)reloadFileViewModel.File).Reload();
          else
          {
            reloadFileViewModel.File.HasUnsavedChanges = true;
          }
        }
        m_files.Clear();
        IsShown = false;
      }
    }

    private  void IgnoreAll()
    {
      lock (m_files)
      {
        foreach (ReloadFileViewModel reloadFileViewModel in m_files.Where(n => n.IsChecked))
          reloadFileViewModel.File.HasUnsavedChanges = true;
        m_files.Clear();
        IsShown = false;
      }
    }

    public override bool IsShown
    {
      get { return base.IsShown; }
      set
      {
        base.IsShown = value;
        if (IsShown)
          SelectedFile = m_files.FirstOrDefault();
      }
    }

    public IObservableCollection<ReloadFileViewModel> Files
    {
      get { return m_files; }
    }

    public ManualCommand IgnorAllCommand
    {
      get { return m_ignorAllCommand; }
    }

    public ManualCommand ReloadAllCommand
    {
      get { return m_reloadAllCommand; }
    }

    public ManualCommand ReloadSelectedCommand
    {
      get { return m_reloadSelectedCommand; }
    }

    public ReloadFileViewModel SelectedFile
    {
      get { return m_selectedFile; }
      set
      {
        if (Equals(value, m_selectedFile)) return;
        m_selectedFile = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand ToggleCommand
    {
      get { return m_toggleCommand; }
    }

    public void AddFile(IFileViewModel file)
    {
      Files.Add(new ReloadFileViewModel(file));
    }
  }
}
