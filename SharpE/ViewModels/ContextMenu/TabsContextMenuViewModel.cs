using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Commands;
using SharpE.ViewModels.Tree;

namespace SharpE.ViewModels.ContextMenu
{
  public class TabsContextMenuViewModel : IContextMenuViewModel
  {
    private readonly MainViewModel m_mainViewModel;
    private readonly List<ContextmenuItemViewModel> m_menuItems = new List<ContextmenuItemViewModel>();
    private readonly ContextmenuItemViewModel m_closeAllContextmenuItemViewModel;
    private readonly ContextmenuItemViewModel m_closeOthersContextmenuItemViewModel;

    public TabsContextMenuViewModel(MainViewModel mainViewModel)
    {
      m_mainViewModel = mainViewModel;
      m_menuItems.Add(new ContextmenuItemViewModel("Close", mainViewModel.CloseFileCommand, mainViewModel, "SelectedFile"));
      m_closeAllContextmenuItemViewModel = new ContextmenuItemViewModel("Close all", new ManualCommand(() => mainViewModel.CloseAllFiles(false)));
      m_menuItems.Add(m_closeAllContextmenuItemViewModel);
      m_closeOthersContextmenuItemViewModel = new ContextmenuItemViewModel("Close others", new ManualCommand(() => mainViewModel.CloseAllFiles(true)));
      m_menuItems.Add(m_closeOthersContextmenuItemViewModel);
      m_mainViewModel.Openfiles.PropertyChanged += OpenfilesOnPropertyChanged;
      m_menuItems.Add(new ContextmenuItemViewModel("Show in project tree", new ManualCommand(ShowInTree)));
      m_menuItems.Add(new ContextmenuItemViewModel("Open containing folder", new ManualCommand(OpenContainingFolder)));
    }

    private void OpenContainingFolder()
    {
      string dir = Path.GetDirectoryName(m_mainViewModel.SelectedFile.Path);
      if (dir != null && Directory.Exists(dir))
        Process.Start(dir);
    }

    private void ShowInTree()
    {
      m_mainViewModel.SelectedTabTree = TabTrees.Project;
      ((ITreeNode)m_mainViewModel.SelectedFile).Show();
      m_mainViewModel.View.Dispatcher.BeginInvoke(new Action(() => { m_mainViewModel.SelectedNode = (ITreeNode) m_mainViewModel.SelectedFile; }), DispatcherPriority.Background);

    }

    private void OpenfilesOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      switch (propertyChangedEventArgs.PropertyName)
      {
        case "Count":
          Refresh();
          break;
      }
    }

    public List<ContextmenuItemViewModel> MenuItems
    {
      get { return m_menuItems; }
    }

    public void Refresh()
    {
      m_closeAllContextmenuItemViewModel.IsVisable = m_mainViewModel.Openfiles.Count > 1;
      m_closeOthersContextmenuItemViewModel.IsVisable = m_mainViewModel.Openfiles.Count > 1;
    }
  }
}
