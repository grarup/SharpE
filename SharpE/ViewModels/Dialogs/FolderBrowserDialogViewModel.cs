using System.Windows.Forms;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Helpers;
using SharpE.Views.Dialogs;

namespace SharpE.ViewModels.Dialogs
{
  internal class FolderBrowserDialogViewModel : BaseDialogViewModel
  {
    private string m_path;
    private string m_title;
    private bool m_canceled;
    private readonly ManualCommand m_browsForFolderCommand;
    private readonly ManualCommand m_okCommand;
    private readonly ManualCommand m_cancelCommand;

    public FolderBrowserDialogViewModel()
    {
      m_browsForFolderCommand = new ManualCommand(BrowseForFolder);
      m_okCommand = new ManualCommand(() => { m_canceled = false; IsShown = false;});
      m_cancelCommand = new ManualCommand(() => { IsShown = false; });
      m_showCloseButton = false;
    }

    public override bool IsShown
    {
      get
      {
        return base.IsShown;
      }
      set
      {
        base.IsShown = value;
        if (IsShown)
          m_canceled = true;
      }
    }

    protected override System.Windows.FrameworkElement GenerateView()
    {
      return new FolderBrowserView {DataContext = this};
    }

    private void BrowseForFolder()
    {
      FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog {SelectedPath = m_path};
      if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
      {
        Path = folderBrowserDialog.SelectedPath;
      }         
    }

    public string Path
    {
      get { return m_path; }
      set
      {
        if (value == m_path) return;
        m_path = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand BrowsForFolderCommand
    {
      get { return m_browsForFolderCommand; }
    }

    public string Title
    {
      get { return m_title; }
      set
      {
        if (value == m_title) return;
        m_title = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand OkCommand
    {
      get { return m_okCommand; }
    }

    public ManualCommand CancelCommand
    {
      get { return m_cancelCommand; }
    }

    public bool Canceled
    {
      get { return m_canceled; }
    }
  }
}
