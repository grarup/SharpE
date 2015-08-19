using System.Collections.Generic;
using System.ComponentModel;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Commands;
using SharpE.ViewModels.Tree;

namespace SharpE.ViewModels.ContextMenu
{
  class TemplateContextMenuViewModel : IContextMenuViewModel
  {
    private readonly MainViewModel m_mainViewModel;
    private readonly List<ContextmenuItemViewModel> m_menuItems = new List<ContextmenuItemViewModel>();
    private readonly ContextmenuItemViewModel m_openContextmenuItemViewModel;
    private readonly ContextmenuItemViewModel m_renameContextmenuItemViewModel;
    private readonly ContextmenuItemViewModel m_newContextmenuItemViewModel;

    public TemplateContextMenuViewModel(MainViewModel mainViewModel)
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
      m_newContextmenuItemViewModel.Children.Add(new ContextmenuItemViewModel("New template", new ManualCommand(CreateTemplate)));
      m_newContextmenuItemViewModel.Children.Add(new ContextmenuItemViewModel("Txt", m_mainViewModel.CreateFileCommand, "file.txt"));
      m_newContextmenuItemViewModel.Children.Add(new ContextmenuItemViewModel("Json", m_mainViewModel.CreateFileCommand, "file.json"));

    }

    private void CreateTemplate()
    {
      IFileViewModel fileViewModel = m_mainViewModel.CreateNewFileAtSelectedNode("newTemplate.template.json");
      fileViewModel.SetContent("{\r\n  \"$schema\" : \"template.schema.json\"\r\n}");
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
  