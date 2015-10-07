using System.Collections.Generic;
using System.ComponentModel;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Commands;
using SharpE.ViewModels.Tree;

namespace SharpE.ViewModels.ContextMenu
{
  class SchemaContextMenuViewModel : IContextMenuViewModel
  {
    private readonly MainViewModel m_mainViewModel;
    private readonly List<MenuItemViewModel> m_menuItems = new List<MenuItemViewModel>();
    private readonly MenuItemViewModel m_openMenuItemViewModel;
    private readonly MenuItemViewModel m_renameMenuItemViewModel;
    private readonly MenuItemViewModel m_newMenuItemViewModel;

    public SchemaContextMenuViewModel(MainViewModel mainViewModel)
    {
      m_mainViewModel = mainViewModel;
      m_mainViewModel.PropertyChanged += MainViewModelOnPropertyChanged;
      m_openMenuItemViewModel = new MenuItemViewModel("Open", mainViewModel.OpenFileViewModelCommand, mainViewModel, "SelectedNode");
      m_menuItems.Add(m_openMenuItemViewModel);
      m_renameMenuItemViewModel = new MenuItemViewModel("Rename", mainViewModel.RenameSelectedNodeCommand);
      m_menuItems.Add(m_renameMenuItemViewModel);
      m_menuItems.Add(new MenuItemViewModel("Delete", mainViewModel.DeleteSelectedNodeCommand));
      m_newMenuItemViewModel = new MenuItemViewModel("New");
      m_menuItems.Add(m_newMenuItemViewModel);
      m_newMenuItemViewModel.Children.Add(new MenuItemViewModel("Folder", m_mainViewModel.CreateFolderCommand));
      m_newMenuItemViewModel.Children.Add(new MenuItemViewModel("New schema", new ManualCommand(CreateSchema)));

    }

    private void CreateSchema()
    {
      IFileViewModel fileViewModel = m_mainViewModel.CreateNewFileAtSelectedNode("newSchema.schema.json");
      fileViewModel.SetContent("{\r\n  \"$schema\" : \"quickschema.schema.json\"\r\n}");
    }

    private void MainViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      switch (propertyChangedEventArgs.PropertyName)
      {
        case "SelectedNode":
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
      if (m_mainViewModel.SelectedNode != null)
      {
        m_openMenuItemViewModel.IsVisable = m_mainViewModel.SelectedNode is IFileViewModel;
        m_renameMenuItemViewModel.IsVisable = true;
        m_newMenuItemViewModel.IsVisable = m_mainViewModel.SelectedNode is DirectoryViewModel;
      }
      else
      {
        m_openMenuItemViewModel.IsVisable = false;
        m_renameMenuItemViewModel.IsVisable = false;
        m_newMenuItemViewModel.IsVisable = false;
        
      }
    }
  }
}
  