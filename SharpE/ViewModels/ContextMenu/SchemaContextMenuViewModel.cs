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
    private readonly List<ContextmenuItemViewModel> m_menuItems = new List<ContextmenuItemViewModel>();
    private readonly ContextmenuItemViewModel m_openContextmenuItemViewModel;
    private readonly ContextmenuItemViewModel m_renameContextmenuItemViewModel;
    private readonly ContextmenuItemViewModel m_newContextmenuItemViewModel;

    public SchemaContextMenuViewModel(MainViewModel mainViewModel)
    {
      m_mainViewModel = mainViewModel;
      m_mainViewModel.PropertyChanged += MainViewModelOnPropertyChanged;
      m_openContextmenuItemViewModel = new ContextmenuItemViewModel("Open", mainViewModel.OpenFileViewModelCommand, mainViewModel, "SelectedNode");
      m_menuItems.Add(m_openContextmenuItemViewModel);
      m_renameContextmenuItemViewModel = new ContextmenuItemViewModel("Rename", mainViewModel.RenameSelectedNodeCommand);
      m_menuItems.Add(m_renameContextmenuItemViewModel);
      m_menuItems.Add(new ContextmenuItemViewModel("Delete", mainViewModel.DeleteSelectedNodeCommand));
      m_newContextmenuItemViewModel = new ContextmenuItemViewModel("New");
      m_menuItems.Add(m_newContextmenuItemViewModel);
      m_newContextmenuItemViewModel.Children.Add(new ContextmenuItemViewModel("Folder", m_mainViewModel.CreateFolderCommand));
      m_newContextmenuItemViewModel.Children.Add(new ContextmenuItemViewModel("New schema", new ManualCommand(CreateSchema)));

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

    public List<ContextmenuItemViewModel> MenuItems
    {
      get { return m_menuItems; }
    }

    public void Refresh()
    {
      if (m_mainViewModel.SelectedNode != null)
      {
        m_openContextmenuItemViewModel.IsVisable = m_mainViewModel.SelectedNode is IFileViewModel;
        m_renameContextmenuItemViewModel.IsVisable = true;
        m_newContextmenuItemViewModel.IsVisable = m_mainViewModel.SelectedNode is DirectoryViewModel;
      }
      else
      {
        m_openContextmenuItemViewModel.IsVisable = false;
        m_renameContextmenuItemViewModel.IsVisable = false;
        m_newContextmenuItemViewModel.IsVisable = false;
        
      }
    }
  }
}
  