using System;
using System.ComponentModel;
using System.Windows;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Helpers;
using SharpE.ViewModels.ContextMenu;
using SharpE.Views.Layout;

namespace SharpE.ViewModels.Layout
{
  public class LayoutElementViewModel : BaseViewModel
  {
    private IEditor m_editor;
    private readonly IObservableCollection<IFileViewModel> m_openFiles = new ObservableCollection<IFileViewModel>();
    private readonly IObservableCollection<IFileViewModel> m_fileUseOrder = new ObservableCollection<IFileViewModel>();
    private IFileViewModel m_selectedFile;
    private readonly MainViewModel m_mainViewModel;
    private readonly int m_index;
    private readonly TabsContextMenuViewModel m_tabsContextMenuViewModel;
    private Action<object> m_dragAction;

    public LayoutElementViewModel(MainViewModel mainViewModel, int index)
    {
      m_mainViewModel = mainViewModel;
      m_index = index;
      m_tabsContextMenuViewModel = new TabsContextMenuViewModel(mainViewModel, this);
    }

    protected override FrameworkElement GenerateView()
    {
      return new LayoutElementView { DataContext = this };
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
          Editor = MainViewModel.EditorManager.GetEditor(m_selectedFile.Exstension, m_index);
          m_editor.File = m_selectedFile;
          m_selectedFile.PropertyChanged += SelectedFileOnPropertyChanged;
        }
        else
        {
          Editor = m_mainViewModel.EditorManager.GetEditor(null, m_index);
          Editor.File = null;
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
      get
      {
        return m_editor ?? (m_editor = m_mainViewModel.EditorManager.GetEditor(null, m_index));
      }
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

    public Action<object, DragDropEffects> DropAction
    {
      get { return Drop; }
    }

    private void Drop(object o, DragDropEffects dragDropEffects)
    {
      IFileViewModel fileViewModel = o as IFileViewModel;
      if (fileViewModel == null) return;
      m_openFiles.Add(fileViewModel);
      m_fileUseOrder.Insert(0, fileViewModel);
      SelectedFile = fileViewModel;
    }

    public Action<object> DragAction
    {
      get { return Drag; }
    }

    private void Drag(object o)
    {
      IFileViewModel fileViewModel = o as IFileViewModel;
      if (fileViewModel == null) return;
      int index = m_openFiles.IndexOf(fileViewModel);
      m_openFiles.Remove(fileViewModel);
      m_fileUseOrder.Remove(fileViewModel);
      if (m_openFiles.Count == 0)
        return;
      if (index >= m_openFiles.Count)
        index = m_openFiles.Count - 1;
      SelectedFile = m_openFiles[index];
    }

    public Action<object, bool> DropCompleteAction
    {
      get { return DropComplete; }
    }

    private void DropComplete(object o, bool arg2)
    {
      if (arg2) return;
      IFileViewModel fileViewModel = o as IFileViewModel;
      if (fileViewModel == null) return;
      m_openFiles.Add(fileViewModel);
      m_fileUseOrder.Insert(0, fileViewModel);
      SelectedFile = fileViewModel;

    }
  }
}
