using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.Templats;
using SharpE.ViewModels.Tree;

namespace SharpE.ViewModels.ContextMenu
{
  class ProjectContextMenuViewModel : IContextMenuViewModel
  {
    private readonly MainViewModel m_mainViewModel;
    private readonly List<MenuItemViewModel> m_menuItems = new List<MenuItemViewModel>();
    private readonly MenuItemViewModel m_openMenuItemViewModel;
    private readonly MenuItemViewModel m_renameMenuItemViewModel;
    private readonly MenuItemViewModel m_newMenuItemViewModel;
    private readonly MenuItemViewModel m_newFromTemplateMenuItemViewModel;

    public ProjectContextMenuViewModel(MainViewModel mainViewModel)
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
      m_newMenuItemViewModel.Children.Add(new MenuItemViewModel("Txt", m_mainViewModel.CreateFileCommand, "file.txt"));
      m_newMenuItemViewModel.Children.Add(new MenuItemViewModel("Json", m_mainViewModel.CreateFileCommand, "file.json"));
      m_newFromTemplateMenuItemViewModel = new MenuItemViewModel("New from template",
        new ConverterObservableCollection<Template, IMenuItemViewModel>(m_mainViewModel.TemplateManager.Templates, template => new MenuItemViewModel("Name", template, new GenericManualCommand<Template>(RunTemplate), template)));
      m_menuItems.Add(m_newFromTemplateMenuItemViewModel);
    }

    private void RunTemplate(Template template)
    {
      template.TargetPath = m_mainViewModel.SelectedNode.Path;
      template.ProjectPath = m_mainViewModel.Path + "\\" + m_mainViewModel.SelectedNode.GetParameter("projectname");
      template.RootPath = m_mainViewModel.Path;
      m_mainViewModel.TemplateDialogViewModel.Template = template;
      m_mainViewModel.DialogHelper.ShowDialog(m_mainViewModel.TemplateDialogViewModel);
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
        m_newFromTemplateMenuItemViewModel.IsVisable = 
          m_newMenuItemViewModel.IsVisable = m_mainViewModel.SelectedNode is DirectoryViewModel;
        
      }
      else
      {
        m_openMenuItemViewModel.IsVisable = false;
        m_renameMenuItemViewModel.IsVisable = false;
        m_newMenuItemViewModel.IsVisable = false;
        m_newFromTemplateMenuItemViewModel.IsVisable = false;
      }
    }
  }
}
  