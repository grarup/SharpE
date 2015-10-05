using System.ComponentModel;
using System.Windows;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Helpers;
using SharpE.ViewModels.ContextMenu;
using SharpE.Views;

namespace SharpE.ViewModels
{
  public class EditorLayoutViewModel : BaseViewModel
  {
    private IEditor m_editor;
    private readonly IObservableCollection<IFileViewModel> m_openFiles = new ObservableCollection<IFileViewModel>();
    private readonly IObservableCollection<IFileViewModel> m_fileUseOrder = new ObservableCollection<IFileViewModel>();
    private IFileViewModel m_selectedFile;
    private readonly MainViewModel m_mainViewModel;
    private readonly TabsContextMenuViewModel m_tabsContextMenuViewModel;

    public EditorLayoutViewModel(MainViewModel mainViewModel)
    {
      m_mainViewModel = mainViewModel;
      m_tabsContextMenuViewModel = new TabsContextMenuViewModel(mainViewModel, this);
    }

    protected override FrameworkElement GenerateView()
    {
      return new EditorLayoutView {DataContext = this};
    }

    public IFileViewModel SelectedFile
    {
      get { return m_selectedFile; }
      set
      {
        if (Equals(value, m_selectedFile)) return;
        if (m_selectedFile != null)
        {
          m_selectedFile.PropertyChanged -= SelectedFileOnPropertyChanged;
        }
        m_selectedFile = value;
        if (m_selectedFile != null)
        {
          if (m_fileUseOrder.Contains(m_selectedFile))
            m_fileUseOrder.Remove(m_selectedFile);
          m_fileUseOrder.Insert(0, m_selectedFile);
          Editor = MainViewModel.EditorManager.GetEditor(m_selectedFile.Exstension);
          m_editor.File = m_selectedFile;
          m_selectedFile.PropertyChanged += SelectedFileOnPropertyChanged;
        }
        else
        {
          Editor = null;
        }
        MainViewModel.SaveFileCommand.Update();
        OnPropertyChanged();
      }
    }

    private void SelectedFileOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      switch (propertyChangedEventArgs.PropertyName)
      {
        case "HasUnsavedChanges":
          MainViewModel.SaveFileCommand.Update();
          MainViewModel.SaveAllFilesCommand.Update();
          break;
      }
    }

    public IObservableCollection<IFileViewModel> OpenFiles
    {
      get { return m_openFiles; }
    }

    public IObservableCollection<IFileViewModel> FileUseOrder
    {
      get { return m_fileUseOrder; }
    }

    public IEditor Editor
    {
      get { return m_editor; }
      set
      {
        if (Equals(value, m_editor)) return;
        m_editor = value;
        OnPropertyChanged();
      }
    }

    public TabsContextMenuViewModel TabsContextMenuViewModel
    {
      get { return m_tabsContextMenuViewModel; }
    }

    public MainViewModel MainViewModel
    {
      get { return m_mainViewModel; }
    }
  }
}
