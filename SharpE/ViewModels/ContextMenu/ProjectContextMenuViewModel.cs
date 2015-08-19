using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
    private readonly List<ContextmenuItemViewModel> m_menuItems = new List<ContextmenuItemViewModel>();
    private readonly ContextmenuItemViewModel m_openContextmenuItemViewModel;
    private readonly ContextmenuItemViewModel m_renameContextmenuItemViewModel;
    private readonly ContextmenuItemViewModel m_newContextmenuItemViewModel;
    private readonly ContextmenuItemViewModel m_newFromTemplateContextmenuItemViewModel;

    public ProjectContextMenuViewModel(MainViewModel mainViewModel)
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
      m_newContextmenuItemViewModel.Children.Add(new ContextmenuItemViewModel("Txt", m_mainViewModel.CreateFileCommand, "file.txt"));
      m_newContextmenuItemViewModel.Children.Add(new ContextmenuItemViewModel("Json", m_mainViewModel.CreateFileCommand, "file.json"));
      m_newFromTemplateContextmenuItemViewModel = new ContextmenuItemViewModel("New from template",
        new ConverterObservableCollection<Template, ContextmenuItemViewModel>(m_mainViewModel.TemplateManager.Templates, template => new ContextmenuItemViewModel("Name", template, new GenericManualCommand<Template>(RunTemplate), template)));
      m_menuItems.Add(m_newFromTemplateContextmenuItemViewModel);
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
        m_newFromTemplateContextmenuItemViewModel.IsVisable = 
          m_newContextmenuItemViewModel.IsVisable = m_mainViewModel.SelectedNode is DirectoryViewModel;
        
      }
      else
      {
        m_openContextmenuItemViewModel.IsVisable = false;
        m_renameContextmenuItemViewModel.IsVisable = false;
        m_newContextmenuItemViewModel.IsVisable = false;
        m_newFromTemplateContextmenuItemViewModel.IsVisable = false;
      }
    }
  }
}
  