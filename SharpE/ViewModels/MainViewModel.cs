using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.Json.AutoComplet;
using SharpE.Json.Data;
using SharpE.Json.Schemas;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Helpers;
using SharpE.Templats;
using SharpE.Templats.ViewModels;
using SharpE.ViewModels.ContextMenu;
using SharpE.ViewModels.Dialogs;
using SharpE.ViewModels.Layout;
using SharpE.ViewModels.Tree;
using SharpE.Views;

namespace SharpE.ViewModels
{
  public class MainViewModel : BaseOwnerViewModel
  {
    #region declerations
    private string m_path;

    private readonly LayoutManager m_layoutManager;

    private TabTrees m_selectedTabTree;
    private readonly List<TabTrees> m_tabTrees;
    private readonly Dictionary<TabTrees, ITreeNode> m_treeNodes = new Dictionary<TabTrees, ITreeNode> { { Tree.TabTrees.Project, null }, { Tree.TabTrees.Schemas, null }, { Tree.TabTrees.Templats, null } };
    private ITreeNode m_root;
    private ITreeNode m_selectedNode;
    private readonly Dictionary<TabTrees, IContextMenuViewModel> m_treeContextMenus = new Dictionary<TabTrees, IContextMenuViewModel> { { Tree.TabTrees.Project, null }, { Tree.TabTrees.Schemas, null }, { Tree.TabTrees.Templats, null } };
    private IContextMenuViewModel m_currentTreeContextMenu;

    private readonly GenericManualCommand<IFileViewModel> m_openFileViewModelCommand;
    private readonly ManualCommand m_openFileCommand;
    private readonly ManualCommand m_newFileCommand;
    private readonly GenericManualCommand<IFileViewModel> m_closeFileCommand;
    private readonly ManualCommand m_openFolderCommand;
    private readonly ManualCommand m_saveFileCommand;
    private readonly ManualCommand m_changeSettingsPathCommand;
    private readonly ManualCommand m_exitCommand;
    private readonly ManualCommand m_renameSelectedNodeCommand;
    private readonly ManualCommand m_renameSelectedNodeDoneCommand;
    private readonly ManualCommand m_renameSelectedNodeCancelCommand;
    private readonly GenericManualCommand<string> m_createFileCommand;
    private readonly ManualCommand m_saveAllFilesCommand;
    private readonly ICommand m_createFolderCommand;
    private readonly ICommand m_deleteSelectedNodeCommand;

    private EditorManager m_editorManager;
    private IFileViewModel m_settings;
    private List<KeyBinding> m_keyBindings;
    private SchemaManager m_schemaManager;

    private readonly IDialogHelper m_dialogHelper;
    private FolderBrowserDialogViewModel m_folderBrowserDialogViewModel;
    private readonly ReloadFilesDialogViewModel m_reloadFilesDialogViewModel;
    private FileSearchDialogViewModel m_fileSearchDialogViewModel;
    private FileSwitchDialogViewModel m_fileSwitchDialogViewModel;
    private TemplateDialogViewModel m_templateDialogViewModel;

    private AutoCompleteCollectionManager m_autoCompleteCollectionManager;
    private string m_title;
    private TemplateManager m_templateManager;
    private string m_runPath;
    private string m_runParameters;

    private readonly MainWindow m_window;

    #endregion

    #region constructor
    public MainViewModel()
    {
      m_window = new MainWindow {DataContext = this};
      m_layoutManager = new LayoutManager(this);

      m_tabTrees = Enum.GetValues(typeof(TabTrees)).Cast<TabTrees>().ToList();
      m_closeFileCommand = new GenericManualCommand<IFileViewModel>(CloseFile);
      m_saveFileCommand = new ManualCommand(Save, () => LayoutManager.ActiveLayoutElement != null && LayoutManager.ActiveLayoutElement.SelectedFile != null && LayoutManager.ActiveLayoutElement.SelectedFile.HasUnsavedChanges);
      m_saveAllFilesCommand = new ManualCommand(SaveAll, () => LayoutManager.LayoutElements.Any(layout => layout.OpenFiles.Any(n => n.HasUnsavedChanges)));
      m_openFileViewModelCommand = new GenericManualCommand<IFileViewModel>(file => OpenFile(file));
      m_openFileCommand = new ManualCommand(OpenFile);
      m_openFolderCommand = new ManualCommand(OpenFolder);
      m_exitCommand = new ManualCommand(() => Environment.Exit(0));
      m_newFileCommand = new ManualCommand(CreateNewFile);
      m_changeSettingsPathCommand = new ManualCommand(ChangeSettingsPath);
      m_renameSelectedNodeCommand = new ManualCommand(() => m_selectedNode.IsRenaming = true);
      m_renameSelectedNodeDoneCommand = new ManualCommand(() => m_selectedNode.IsRenaming = false);
      m_renameSelectedNodeCancelCommand = new ManualCommand(() => { m_selectedNode.RenameString = null; m_selectedNode.IsRenaming = false; });
      m_createFileCommand = new GenericManualCommand<string>(s => CreateNewFileAtSelectedNode(s));
      m_reloadFilesDialogViewModel = new ReloadFilesDialogViewModel();
      m_createFolderCommand = new ManualCommand(() => CreateFolder("Newfolder"));
      m_deleteSelectedNodeCommand = new ManualCommand(DeleteSelectedNode);

      m_dialogHelper = new DialogHelper(this);
      ServiceProvider.Registre<IDialogHelper>(DialogHelper);

      if (string.IsNullOrEmpty(Properties.Settings.Default.SettingPath) ||
          !Directory.Exists(Properties.Settings.Default.SettingPath))
      {
        m_folderBrowserDialogViewModel = new FolderBrowserDialogViewModel();
        m_folderBrowserDialogViewModel.PropertyChanged += StartFolderBrowserDialogViewModelOnPropertyChanged;
        m_folderBrowserDialogViewModel.Title = "Select settings path";
        m_folderBrowserDialogViewModel.Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\sharpE\\settings";
        DialogHelper.ShowDialog(m_folderBrowserDialogViewModel);
      }
      else
      {
        Init();
      }
    }

    private void DeleteSelectedNode()
    {
      try
      {
        ITreeNode node = m_selectedNode;
        ITreeNode parrent = node.Parent;
        int index = parrent == null ? 0 : parrent.Children.IndexOf(node);
        node.Delete();
        if (LayoutManager.LayoutElements.Any(layout => layout.OpenFiles.Any(n => n == node)))
          CloseFile((IFileViewModel) node);
        if (parrent == null)
          return;
        if (index >= parrent.Children.Count - 2)
          index = parrent.Children.Count - 2;
        SelectedNode = index >= 0 ? parrent.Children[index] : parrent;
      }
      catch (Exception e)
      {
        m_dialogHelper.ShowMessageBox("Could not delete " + m_selectedNode.Name, e.Message);
      }
    }

    private DirectoryViewModel CreateFolder(string newName)
    {
      DirectoryViewModel directoryViewModel = m_selectedNode as DirectoryViewModel;
      if (directoryViewModel == null)
        return null;
      directoryViewModel.IsExpanded = true;
      int index = 1;
      while (directoryViewModel.Children.Any(n => n.Name == newName))
      {
        string name = System.IO.Path.GetFileNameWithoutExtension(newName);
        if (name == null)
          return null;
        if (index > 1)
          name = name.Substring(0, name.Length - ((index - 1).ToString().Length));
        newName = name + index + System.IO.Path.GetExtension(newName);
        index++;
      }
      DirectoryViewModel newDirectory = directoryViewModel.CreateDirectory(newName);
      if (newDirectory == null)
        m_dialogHelper.ShowMessageBox("Can not create folder.","Folder " + m_selectedNode.Path + "\\Newfolder could not be created");
      else
      {
        SelectedNode = newDirectory;
        newDirectory.IsRenaming = true;
      }
      return newDirectory;
    }

    #endregion

    #region private methods
    public IFileViewModel CreateNewFileAtSelectedNode(string obj)
    {
      m_selectedNode.IsExpanded = true;
      int index = 1;
      while (m_selectedNode.Children.Any(n => n.Name == obj))
      {
        string name = System.IO.Path.GetFileNameWithoutExtension(obj);
        if (name == null)
          return null;
        if (index > 1)
          name = name.Substring(0, name.Length - ((index - 1).ToString().Length));
        obj = name + index + System.IO.Path.GetExtension(obj);
        index++;
      }
      FileViewModel fileViewModel = new FileViewModel(null, m_selectedNode) { Name = obj };
      m_selectedNode.Children.Add(fileViewModel);
      fileViewModel.HasUnsavedChanges = true;
      Save(fileViewModel);
      SelectedNode = fileViewModel;
      OpenFile(fileViewModel);
      fileViewModel.IsRenaming = true;
      return fileViewModel;
    }

    private void OpenFile()
    {
      FileDialog fileDialog = new OpenFileDialog();
      if (fileDialog.ShowDialog() == DialogResult.OK)
      {
        foreach (string fileName in fileDialog.FileNames)
        {
          OpenFile(fileName);
        }
      }
    }

    private void ChangeSettingsPath()
    {
      if (m_folderBrowserDialogViewModel == null)
        m_folderBrowserDialogViewModel = new FolderBrowserDialogViewModel();
      m_folderBrowserDialogViewModel.PropertyChanged += FolderBrowserDialogViewModelOnPropertyChanged;
      m_folderBrowserDialogViewModel.Title = "Select settings path";
      m_folderBrowserDialogViewModel.Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\sharpE\\settings";
      DialogHelper.ShowDialog(m_folderBrowserDialogViewModel);
    }

    private void FolderBrowserDialogViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "IsShown":
          if (!m_folderBrowserDialogViewModel.IsShown)
          {
            if (!m_folderBrowserDialogViewModel.Canceled)
            {
              if (!Directory.Exists(m_folderBrowserDialogViewModel.Path))
                Directory.CreateDirectory(m_folderBrowserDialogViewModel.Path);
              Properties.Settings.Default.SettingPath = m_folderBrowserDialogViewModel.Path;
              Properties.Settings.Default.Save();
              Init();
            }
          }
          break;
      }
    }

    private void OpenFolder()
    {
      FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog { SelectedPath = m_path };
      if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        Path = folderBrowserDialog.SelectedPath;
    }

    private void StartFolderBrowserDialogViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      switch (propertyChangedEventArgs.PropertyName)
      {
        case "IsShown":
          if (!m_folderBrowserDialogViewModel.IsShown)
          {
            if (m_folderBrowserDialogViewModel.Canceled)
            {
              if (!Directory.Exists(Properties.Settings.Default.SettingPath))
              {
                Properties.Settings.Default.SettingPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\sharpE\\settings";
                Properties.Settings.Default.Save();
                Directory.CreateDirectory(Properties.Settings.Default.SettingPath);
              }
            }
            else
            {
              if (!Directory.Exists(m_folderBrowserDialogViewModel.Path))
                Directory.CreateDirectory(m_folderBrowserDialogViewModel.Path);
              Properties.Settings.Default.SettingPath = m_folderBrowserDialogViewModel.Path;
              Properties.Settings.Default.Save();
            }
            Init();
          }
          break;
      }
    }

    public IFileViewModel GetSetting(string path)
    {
      return m_treeNodes[Tree.TabTrees.Settings].GetFile(path);
    }

    private void Init()
    {
      if (!Directory.Exists(Properties.Settings.Default.SettingPath))
        Directory.CreateDirectory(Properties.Settings.Default.SettingPath);
      if (!File.Exists(Properties.Settings.Default.SettingPath + "\\settings.json"))
        File.WriteAllBytes(Properties.Settings.Default.SettingPath + "\\settings.json", Properties.Resources.settings);
      m_treeNodes.Add(Tree.TabTrees.Settings, new DirectoryViewModel(Properties.Settings.Default.SettingPath));
      Settings = GetSetting(Properties.Settings.Default.SettingPath + "\\settings.json");

      m_schemaManager = new SchemaManager(m_settings);
      m_schemaManager.AddLockedSchema(new Schema(Properties.Resources.generalsettings_schema, m_schemaManager));
      m_schemaManager.AddLockedSchema(new Schema(Properties.Resources.basetexteditorsettings_schema, m_schemaManager));
      m_schemaManager.AddLockedSchema(new Schema(Properties.Resources.jsoneditor_schema, m_schemaManager));
      m_schemaManager.AddLockedSchema(new Schema(Properties.Resources.template_schema, m_schemaManager));

      m_autoCompleteCollectionManager = new AutoCompleteCollectionManager(m_schemaManager);
      ServiceProvider.Registre<IAutoCompleteCollectionManager>(m_autoCompleteCollectionManager);

      Path = Properties.Settings.Default.ProjectPath;

      EditorManager = new EditorManager(m_settings, this);

      m_templateManager = new TemplateManager(m_settings);
      m_treeNodes[Tree.TabTrees.Templats] = TemplateManager.Root;

      m_treeNodes[Tree.TabTrees.Schemas] = new CollectionTreeViewModel(m_schemaManager.Paths);
      m_layoutManager.LoadSetup();

      UpdateSettings();
      
      Root = m_treeNodes[m_selectedTabTree];

      m_treeContextMenus[Tree.TabTrees.Project] = new ProjectContextMenuViewModel(this);
      m_treeContextMenus[Tree.TabTrees.Schemas] = new SchemaContextMenuViewModel(this);
      m_treeContextMenus[Tree.TabTrees.Templats] = new TemplateContextMenuViewModel(this);
      m_currentTreeContextMenu = m_treeContextMenus[Tree.TabTrees.Project];
    }

    private void UpdateSettings()
    {
      JsonException jsonException;
      JsonNode settings = JsonHelperFunctions.Parse(Settings.GetContent<string>(), out jsonException) as JsonNode;
      if (jsonException != null || settings == null)
        return;
      JsonArray keybindings = settings.GetObjectOrDefault<object>("shortcuts", null) as JsonArray;
      KeyBindings = keybindings != null ? KeyboardBindingGenerator.GenerateKeyBinding(keybindings, this).ToList() : new List<KeyBinding>();
      m_runPath = settings.GetObjectOrDefault<string>("runpath", null);
      m_runParameters = settings.GetObjectOrDefault<string>("runparameters", null);
    }

    private void CloseFile(IFileViewModel file)
    {
      if (file == null)
        return;
      if (!LayoutManager.LayoutElements.Any(layout => layout.OpenFiles.Contains(file)))
        return;
      if (file.HasUnsavedChanges)
      {
        DialogHelper.ShowMessageBox(SaveOnCloseMessageBoxReply, new object[] { file }, "Has unsaved data",
                                    "Save unsaved data?", MessageBoxButton.YesNoCancel);
      }
      else
        SaveOnCloseMessageBoxReply(MessageBoxResult.No, new object[] { file });
    }

    private void SaveOnCloseMessageBoxReply(MessageBoxResult arg1, object[] arg2)
    {
      IFileViewModel file = (IFileViewModel)arg2[0];
      switch (arg1)
      {
        case MessageBoxResult.Yes:
          Save(file);
          break;
        case MessageBoxResult.No:
          break;
        default:
          return;
      }
      file.Reset();
      LayoutElementViewModel layoutElementViewModel = LayoutManager.LayoutElements.FirstOrDefault(n => n.OpenFiles.Contains(file));
      if (layoutElementViewModel == null)
        return;
      layoutElementViewModel.FileUseOrder.Remove(file);
      if (layoutElementViewModel.SelectedFile == file)
      {
        layoutElementViewModel.OpenFiles.Remove(file);
        layoutElementViewModel.SelectedFile = layoutElementViewModel.FileUseOrder.FirstOrDefault();
      }
      else
      {
        layoutElementViewModel.OpenFiles.Remove(file);
      }
      file.FileChangedOnDisk -= FileOnFileChangedOnDisk;
    }

    public void OpenFile(IFileViewModel file, LayoutElementViewModel layoutElement = null, bool select = true)
    {
      if (file == null)
        return;
      if (layoutElement == null)
        layoutElement = m_layoutManager.LayoutElements.FirstOrDefault(n => n.OpenFiles.Contains(file)) ?? LayoutManager.ActiveLayoutElement;
      if (file.Path == null || !layoutElement.OpenFiles.Any(n => n.Path == file.Path))
      {
        file.FileChangedOnDisk += FileOnFileChangedOnDisk;
        layoutElement.OpenFiles.Add(file);
      }
      if (select)
        layoutElement.SelectedFile = file;
    }

    private void FileOnFileChangedOnDisk(IFileViewModel sender)
    {
      if (!m_view.Dispatcher.CheckAccess())
      {
        m_view.Dispatcher.BeginInvoke(new Action(() => FileOnFileChangedOnDisk(sender)));
        return;
      }

      if (!sender.IsBasedOnCurrentFile())
      {
        m_reloadFilesDialogViewModel.AddFile(sender);
        if (!m_reloadFilesDialogViewModel.IsShown)
          m_dialogHelper.ShowDialog(m_reloadFilesDialogViewModel);
      }
    }



    private async Task<bool> OnClosing()
    {
      if (MessageBoxViewModel.IsShown)
        return false;
      if (LayoutManager.LayoutElements.Any(layout => layout.OpenFiles.Any(n => n.HasUnsavedChanges)))
      {
        MessageBoxResult result =
          await
          DialogHelper.ShowMessageBox("UnsavedChanges", "The following files have unsaved changes:\r\n" +
                                                        string.Join("\r\n", LayoutManager.LayoutElements.SelectMany(layout => layout.OpenFiles).Where(n => n.HasUnsavedChanges).Select(n => "  " + n.Name)),
                                      MessageBoxButton.YesNoCancel);
        switch (result)
        {
          case MessageBoxResult.Cancel:
            return false;
          case MessageBoxResult.Yes:
            SaveAll();
            break;
          case MessageBoxResult.No:
            break;
        }
      }
      m_autoCompleteCollectionManager.Save();
      m_layoutManager.SaveSetup();
      return true;
    }

    public void Run()
    {
      try
      {
        ProcessStartInfo startInfo = new ProcessStartInfo(m_runPath)
        {
          WorkingDirectory = System.IO.Path.GetDirectoryName(m_runPath),
          Arguments = m_runParameters
        };
        Process process = new Process { StartInfo = startInfo};
        process.Start();
      }
      catch (Exception e)
      {
        m_dialogHelper.ShowMessageBox("Can't run", e.Message);
      }
    }



    #endregion

    #region public methods
    public void CreateNewFile()
    {
      FileViewModel fileViewModel = new FileViewModel(null);
      LayoutManager.ActiveLayoutElement.OpenFiles.Add(fileViewModel);
      LayoutManager.ActiveLayoutElement.SelectedFile = fileViewModel;
    }

    public void CloseAllFiles(bool excludeSelected)
    {
      List<IFileViewModel> unsavedFiles = LayoutManager.ActiveLayoutElement.OpenFiles.Where(n => n.HasUnsavedChanges).ToList();
      if (unsavedFiles.Count > 0)
      {
        m_dialogHelper.ShowMessageBox(CloseAllMessageboxResult, new object[] {unsavedFiles, excludeSelected},
          "Unsaved files",
          "These files have unsaved changes, save?\r\n" + string.Join(",\r\n", unsavedFiles.Select(n => n.Name)),
          MessageBoxButton.YesNoCancel);
      }
      else
        CloseAllMessageboxResult(MessageBoxResult.None, new object[] {null, excludeSelected});
    }

    private void CloseAllMessageboxResult(MessageBoxResult arg1, object[] arg2)
    {
      switch (arg1)
      {
        case MessageBoxResult.None:
          break;
        case MessageBoxResult.Yes:
          foreach (IFileViewModel fileViewModel in (IEnumerable<IFileViewModel>)arg2[0])
            Save(fileViewModel);
          break;
        case MessageBoxResult.No:
          foreach (IFileViewModel fileViewModel in (IEnumerable<IFileViewModel>)arg2[0])
            fileViewModel.Reset();
          break;
        default:
          return;
      }
      List<IFileViewModel> files = LayoutManager.ActiveLayoutElement.OpenFiles.ToList();
      foreach (IFileViewModel fileViewModel in files)
      {
        if (!((bool)arg2[1]) || fileViewModel != LayoutManager.ActiveLayoutElement.SelectedFile)
          CloseFile(fileViewModel);
      }
    }

    public void ShowSwitchFile()
    {
      if (LayoutManager.ActiveLayoutElement.OpenFiles.Count < 2)
        return;
      if (m_fileSwitchDialogViewModel == null)
        m_fileSwitchDialogViewModel = new FileSwitchDialogViewModel(LayoutManager.ActiveLayoutElement);
      if (m_fileSwitchDialogViewModel.IsShown)
        return;
      DialogHelper.ShowDialog(m_fileSwitchDialogViewModel);
    }

    public void OpenFileSearch()
    {
      if (m_fileSearchDialogViewModel == null)
      {
        m_fileSearchDialogViewModel = new FileSearchDialogViewModel(this);
      }
      m_fileSearchDialogViewModel.DirectoryViewModel = (DirectoryViewModel)m_treeNodes[Tree.TabTrees.Project].Children[0];
      m_fileSearchDialogViewModel.SearchString = "";
      DialogHelper.ShowDialog(m_fileSearchDialogViewModel);
    }

    public IFileViewModel OpenFile(string path, LayoutElementViewModel layoutElement = null, bool select = true)
    {
      IFileViewModel fileViewModel = null;
      foreach (ITreeNode treeNode in m_treeNodes.Values)
      {
        if (treeNode == null)
          continue;
        fileViewModel = treeNode.GetFile(path);
        if (fileViewModel != null)
          break;
      }
      if (fileViewModel == null)
      {
        IEditor editor = m_editorManager.BaseEditors.FirstOrDefault(n => n.Settings != null && n.Settings.Path == path);
        if (editor != null)
          fileViewModel = editor.Settings;
      }

      if (fileViewModel == null)
        fileViewModel = new FileViewModel(path);
      OpenFile(fileViewModel, layoutElement, select);
      return fileViewModel;
    }

    public void Save()
    {
      Save(LayoutManager.ActiveLayoutElement.SelectedFile);
    }

    public void SaveAll()
    {
      foreach (FileViewModel fileViewModel in LayoutManager.LayoutElements.SelectMany(n => n.OpenFiles))
        Save(fileViewModel);
    }

    public void Save(IFileViewModel fileViewModel)
    {
      try
      {
        fileViewModel.Save();
      }
      catch (Exception e)
      {
        m_dialogHelper.ShowMessageBox("Save failed!", "Could not save:\r\n" + e.Message);
      }
    }

    public void FindInFile()
    {
      LayoutElementViewModel layoutElement = LayoutManager.LayoutElements.FirstOrDefault(l => l.OpenFiles.Contains(m_editorManager.FindInFilesViewModel));
      if (layoutElement == null)
      {
        LayoutManager.ActiveLayoutElement.OpenFiles.Add(m_editorManager.FindInFilesViewModel);
        layoutElement = LayoutManager.ActiveLayoutElement;
      }
      m_editorManager.FindInFilesViewModel.TreeNode = m_root;
      layoutElement.SelectedFile = m_editorManager.FindInFilesViewModel;
    }
    #endregion

    #region public properties
    public ITreeNode Root
    {
      get { return m_root; }
      set
      {
        if (Equals(value, m_root)) return;
        m_root = value;
        OnPropertyChanged();
      }
    }

    public ITreeNode SelectedNode
    {
      get { return m_selectedNode; }
      set
      {
        if (Equals(value, m_selectedNode)) return;
        if (m_selectedNode != null)
          m_selectedNode.IsRenaming = false;
        m_selectedNode = value;
        if (m_selectedNode != null)
          m_selectedNode.Show();
        OnPropertyChanged();
      }
    }

    public string Path
    {
      get { return m_path; }
      set
      {
        if (value == m_path) return;
        m_path = value;
        Properties.Settings.Default.ProjectPath = m_path;
        Properties.Settings.Default.Save();
        if (Directory.Exists(m_path))
        {
          m_treeNodes[Tree.TabTrees.Project] = new CollectionTreeViewModel();
          m_treeNodes[Tree.TabTrees.Project].Children.Add(new DirectoryViewModel(Path) {IsExpanded = true});
          if (m_selectedTabTree == Tree.TabTrees.Project)
            Root = m_treeNodes[Tree.TabTrees.Project];
          AutoCompleteCollectionManager.Project = m_treeNodes[Tree.TabTrees.Project];
          Title = m_treeNodes[Tree.TabTrees.Project].Children[0].Name;

        }
        OnPropertyChanged();
      }
    }

 
    public TabTrees SelectedTabTree
    {
      get { return m_selectedTabTree; }
      set
      {
        if (value == m_selectedTabTree) return;
        m_selectedTabTree = value;
        Root = m_treeNodes[m_selectedTabTree];
        CurrentTreeContextMenu = m_treeContextMenus.ContainsKey(m_selectedTabTree) ? m_treeContextMenus[m_selectedTabTree] : null;
        OnPropertyChanged();
      }
    }

    public List<TabTrees> TabTrees
    {
      get { return m_tabTrees; }
    }

    public IEnumerable<KeyBinding> KeyBindings
    {
      get { return m_keyBindings; }
      private set
      {
        if (Equals(value, m_keyBindings)) return;
        m_keyBindings = value.ToList();
        OnPropertyChanged();
      }
    }

    public EditorManager EditorManager
    {
      get { return m_editorManager; }
      set
      {
        if (Equals(value, m_editorManager)) return;
        m_editorManager = value;
        OnPropertyChanged();
      }
    }

    public IFileViewModel Settings
    {
      get { return m_settings; }
      set
      {
        if (Equals(value, m_settings)) return;
        m_settings = value;
        OnPropertyChanged();
      }
    }

    public string Title
    {
      get { return m_title; }
      set
      {
        if (value == m_title) return;
        m_title = value;
        OnPropertyChanged();
      }
    }

    public SchemaManager SchemaManager
    {
      get { return m_schemaManager; }
    }

    public Func<Task<bool>> ClosingTask
    {
      get { return OnClosing; }
    }

    public IDialogHelper DialogHelper
    {
      get { return m_dialogHelper; }
    }

    public AutoCompleteCollectionManager AutoCompleteCollectionManager
    {
      get { return m_autoCompleteCollectionManager; }
    }

    public GenericManualCommand<string> CreateFileCommand
    {
      get { return m_createFileCommand; }
    }

    public ICommand OpenFileViewModelCommand
    {
      get { return m_openFileViewModelCommand; }
    }

    public ICommand CloseFileCommand
    {
      get { return m_closeFileCommand; }
    }

    public ManualCommand SaveFileCommand
    {
      get { return m_saveFileCommand; }
    }

    public ManualCommand SaveAllFilesCommand
    {
      get { return m_saveAllFilesCommand; }
    }

    public ManualCommand OpenFolderCommand
    {
      get { return m_openFolderCommand; }
    }

    public ManualCommand ChangeSettingsPathCommand
    {
      get { return m_changeSettingsPathCommand; }
    }

    public ManualCommand ExitCommand
    {
      get { return m_exitCommand; }
    }

    public ManualCommand OpenFileCommand
    {
      get { return m_openFileCommand; }
    }

    public ManualCommand NewFileCommand
    {
      get { return m_newFileCommand; }
    }

    public ManualCommand RenameSelectedNodeCommand
    {
      get { return m_renameSelectedNodeCommand; }
    }

    public ManualCommand RenameSelectedNodeDoneCommand
    {
      get { return m_renameSelectedNodeDoneCommand; }
    }

    public ManualCommand RenameSelectedNodeCancelCommand
    {
      get { return m_renameSelectedNodeCancelCommand; }
    }

    public IContextMenuViewModel CurrentTreeContextMenu
    {
      get { return m_currentTreeContextMenu; }
      set
      {
        if (Equals(value, m_currentTreeContextMenu)) return;
        m_currentTreeContextMenu = value;
        OnPropertyChanged();
      }
    }

    public TemplateManager TemplateManager
    {
      get { return m_templateManager; }
    }

    public TemplateDialogViewModel TemplateDialogViewModel
    {
      get
      {
        if (m_templateDialogViewModel == null)
        {
          m_templateDialogViewModel = new TemplateDialogViewModel();
        }
        return m_templateDialogViewModel;
      }
    }

    public ICommand CreateFolderCommand
    {
      get { return m_createFolderCommand; }
    }

    public ICommand DeleteSelectedNodeCommand
    {
      get { return m_deleteSelectedNodeCommand; }
    }

    public LayoutManager LayoutManager
    {
      get { return m_layoutManager; }
    }

    public MainWindow Window
    {
      get { return m_window; }
    }
    #endregion
  }
}
