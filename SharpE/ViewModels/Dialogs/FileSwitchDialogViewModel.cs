using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SharpE.Definitions;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Helpers;
using SharpE.Views;
using SharpE.Views.Dialogs;

namespace SharpE.ViewModels.Dialogs
{
  class FileSwitchDialogViewModel : BaseDialogViewModel
  {
    private readonly IObservableCollection<IFileViewModel> m_files;
    private IFileViewModel m_selectedFile;
    private readonly MainViewModel m_mainViewModel;
    private readonly GenericManualCommand<int> m_switchFileCommand;


    public FileSwitchDialogViewModel(MainViewModel mainViewModel, IObservableCollection<IFileViewModel> files )
    {
      m_mainViewModel = mainViewModel;
      m_files = files;
      m_switchFileCommand = new GenericManualCommand<int>(SwitchFile, null, Converter);
      m_showCloseButton = false;
    }

    private int Converter(object o)
    {
      var s = o as string;
      if (s != null)
        return int.Parse(s);
      return 0;
    }

    private void SwitchFile(int diff)
    {
      int index = m_files.IndexOf(m_selectedFile);
      if (diff > 0)
        SelectedFile = index > 0 ? m_files[index - 1] : m_files.Last();
      else
        SelectedFile = index < m_files.Count - 1 ? m_files[index + 1] : m_files.First();
    }

    protected override FrameworkElement GenerateView()
    {
      FileSwitchView fileSwitchView = new FileSwitchView {DataContext = this};
      fileSwitchView.PreviewKeyUp += FileSwitchViewOnKeyUp;
      return fileSwitchView;
    }


    private void FileSwitchViewOnKeyUp(object sender, KeyEventArgs keyEventArgs)
    {
      switch (keyEventArgs.Key)
      {
        case Key.RightCtrl:
        case Key.LeftCtrl:
          IsShown = false;
          break;
      }
    }

    public override bool IsShown
    {
      get { return base.IsShown; }
      set
      {
        if (IsShown == value) return;
        base.IsShown = value;
        if (IsShown)
        {
          SelectedFile = m_files.Count > 1 ? m_files[1] : null;
        }
        else
        {
          m_mainViewModel.SelectedFile = m_selectedFile;
        }
      }
    }

    public IObservableCollection<IFileViewModel> Files
    {
      get { return m_files; }
    }

    public IFileViewModel SelectedFile
    {
      get { return m_selectedFile; }
      set
      {
        if (Equals(value, m_selectedFile)) return;
        m_selectedFile = value;
        OnPropertyChanged();
      }
    }

    public GenericManualCommand<int> SwitchFileCommand
    {
      get { return m_switchFileCommand; }
    }
  }
}
