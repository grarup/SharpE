using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Helpers;
using SharpE.ViewModels.Tree;
using SharpE.Views.Dialogs;

namespace SharpE.ViewModels.Dialogs
{
  public class FileSearchDialogViewModel : BaseDialogViewModel
  {
    private readonly MainViewModel m_mainViewModel;
    private string m_searchString = "";
    private DirectoryViewModel m_directoryViewModel;
    private readonly FilteredObservableCollection<FileViewModel> m_filteredFiles;
    private FileSearchView m_fileSearchView;
    private readonly ManualCommand m_openCommand;
    private readonly GenericManualCommand<int> m_changeIndexCommand;
    private int m_index;
    private int m_shortenPath;
    private readonly FileItemRender m_fileItemRender = new FileItemRender();


    public FileSearchDialogViewModel(MainViewModel mainViewModel)
    {
      m_mainViewModel = mainViewModel;
      m_filteredFiles = new FilteredObservableCollection<FileViewModel>(null, n => n.Name.Contains(m_searchString));
      m_showCloseButton = false;
      m_openCommand = new ManualCommand(OpenFile);
      m_changeIndexCommand = new GenericManualCommand<int>(ChangeIndex, null, n => int.Parse((string) n));
    }

    private void ChangeIndex(int diff)
    {
      int newIndex = m_index + diff;
      if (newIndex < 0)
        newIndex = 0;
      if (newIndex >= m_filteredFiles.Count)
        newIndex = m_filteredFiles.Count - 1;
      Index = newIndex;
    }

    private void OpenFile()
    {
      m_mainViewModel.OpenFileViewModelCommand.Execute(m_filteredFiles[m_index]);
      IsShown = false;
    }

    protected override System.Windows.FrameworkElement GenerateView()
    {
      m_fileSearchView = new FileSearchView {DataContext = this};
      return m_fileSearchView;
    }

    public string SearchString
    {
      get { return m_searchString; }
      set
      {
        if (value == m_searchString) return;
        m_searchString = value;
        m_filteredFiles.Update();
        OnPropertyChanged();
      }
    }

    public DirectoryViewModel DirectoryViewModel
    {
      get { return m_directoryViewModel; }
      set
      {
        if (Equals(value, m_directoryViewModel)) return;
        m_directoryViewModel = value;
        ShortenPath = m_directoryViewModel.Path.Length;
        m_filteredFiles.BaseObservableCollection = m_directoryViewModel.AllFiles;
        OnPropertyChanged();
      }
    }

    public FilteredObservableCollection<FileViewModel> FilteredFiles
    {
      get { return m_filteredFiles; }
    }

    public ManualCommand OpenCommand
    {
      get { return m_openCommand; }
    }

    public int Index
    {
      get { return m_index; }
      set
      {
        if (value == m_index) return;
        m_index = value;
        OnPropertyChanged();
      }
    }

    public GenericManualCommand<int> ChangeIndexCommand
    {
      get { return m_changeIndexCommand; }
    }

    public int ShortenPath
    {
      get { return m_shortenPath; }
      set
      {
        if (value == m_shortenPath) return;
        m_shortenPath = value;
        m_fileItemRender.ShortenPath = m_shortenPath;
        OnPropertyChanged();
      }
    }

    public FileItemRender FileItemRender
    {
      get { return m_fileItemRender; }
    }
  }
}
