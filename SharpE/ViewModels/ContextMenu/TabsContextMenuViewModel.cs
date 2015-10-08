using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using SharpE.MvvmTools.Commands;
using SharpE.ViewModels.Layout;
using SharpE.ViewModels.Tree;

namespace SharpE.ViewModels.ContextMenu
{
  public class TabsContextMenuViewModel : IContextMenuViewModel
  {
    private readonly MainViewModel m_mainViewModel;
    private readonly LayoutElementViewModel m_layoutElement;
    private readonly List<MenuItemViewModel> m_menuItems = new List<MenuItemViewModel>();
    private readonly MenuItemViewModel m_closeAllMenuItemViewModel;
    private readonly MenuItemViewModel m_closeOthersMenuItemViewModel;

    public TabsContextMenuViewModel(MainViewModel mainViewModel, LayoutElementViewModel layoutElement)
    {
      m_mainViewModel = mainViewModel;
      m_layoutElement = layoutElement;
      m_menuItems.Add(new MenuItemViewModel("Close", mainViewModel.CloseFileCommand, layoutElement, "SelectedFile"));
      m_menuItems.Add(new MenuItemViewModel("Revert", new ManualCommand(() => layoutElement.SelectedFile.Reload())));
      m_closeAllMenuItemViewModel = new MenuItemViewModel("Close all", new ManualCommand(() => mainViewModel.CloseAllFiles(false)));
      m_menuItems.Add(m_closeAllMenuItemViewModel);
      m_closeOthersMenuItemViewModel = new MenuItemViewModel("Close others", new ManualCommand(() => mainViewModel.CloseAllFiles(true)));
      m_menuItems.Add(m_closeOthersMenuItemViewModel);
      m_layoutElement.OpenFiles.PropertyChanged += OpenfilesOnPropertyChanged;
      m_menuItems.Add(new MenuItemViewModel("Show in project tree", new ManualCommand(ShowInTree)));
      m_menuItems.Add(new MenuItemViewModel("Open containing folder", new ManualCommand(OpenContainingFolder)));
    }

    private void OpenContainingFolder()
    {
      string dir = Path.GetDirectoryName(m_layoutElement.SelectedFile.Path);
      if (dir != null && Directory.Exists(dir))
        Process.Start(dir);
    }

    private void ShowInTree()
    {
      m_mainViewModel.SelectedTabTree = TabTrees.Project;
      m_layoutElement.SelectedFile.Show();
      m_mainViewModel.View.Dispatcher.BeginInvoke(new Action(() => { m_mainViewModel.SelectedNode = m_layoutElement.SelectedFile; }), DispatcherPriority.Background);

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

    public List<MenuItemViewModel> MenuItems
    {
      get { return m_menuItems; }
    }

    public void Refresh()
    {
      m_closeAllMenuItemViewModel.IsVisable = m_layoutElement.OpenFiles.Count > 1;
      m_closeOthersMenuItemViewModel.IsVisable = m_layoutElement.OpenFiles.Count > 1;
    }
  }
}
