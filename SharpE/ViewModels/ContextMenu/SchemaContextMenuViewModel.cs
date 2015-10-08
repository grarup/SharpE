using System;
using System.Collections.Generic;
using System.ComponentModel;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.MvvmTools.Commands;
using SharpE.ViewModels.Layout;
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
    private readonly MenuItemViewModel m_addSchemaMenuItemViewModel;
    private LayoutElementViewModel m_currentLayoutViewModel;

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
      m_addSchemaMenuItemViewModel = new MenuItemViewModel("Add schema to file", new ManualCommand(AddSchema));
      m_menuItems.Add(m_addSchemaMenuItemViewModel);
      m_menuItems.Add(m_newMenuItemViewModel);
      m_newMenuItemViewModel.Children.Add(new MenuItemViewModel("Folder", m_mainViewModel.CreateFolderCommand));
      m_newMenuItemViewModel.Children.Add(new MenuItemViewModel("New schema", new ManualCommand(CreateSchema)));

    }

    private void AddSchema()
    {
      string text = m_mainViewModel.LayoutManager.ActiveLayoutElement.SelectedFile.GetContent<string>();
      int startIndex = text.IndexOf("{");
      int lineIndex = text.IndexOf("\n");
      while (lineIndex < startIndex)
        lineIndex = text.IndexOf("\n", lineIndex + 1);
      text = text.Substring(0, lineIndex) + "\"$schema\" : \"" + m_mainViewModel.SelectedNode.Name + "\",\n" + text.Substring(lineIndex + 1, text.Length - lineIndex -1);
      m_mainViewModel.LayoutManager.ActiveLayoutElement.SelectedFile.SetContent(text);
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
        m_addSchemaMenuItemViewModel.IsVisable = m_mainViewModel.LayoutManager.ActiveLayoutElement.SelectedFile.Exstension == ".json";
      }
      else
      {
        m_openMenuItemViewModel.IsVisable = false;
        m_renameMenuItemViewModel.IsVisable = false;
        m_newMenuItemViewModel.IsVisable = false;
        m_addSchemaMenuItemViewModel.IsVisable = false;

      }
    }
  }
}
  