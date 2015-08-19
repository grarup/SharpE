using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Properties;

namespace SharpE.ViewModels.Tree
{
  public class DirectoryViewModel : ITreeNode, IDisposable
  {
    private readonly IObservableCollection<ITreeNode> m_children;
    private string m_name;
    private FileSystemWatcher m_fileSystemWatcher;
    private readonly ITreeNode m_parrent;
    private string m_path;
    private readonly Dictionary<string, DateTime> m_externalChanges;
    private Timer m_cleanUpTimer;
    private readonly IObservableCollection<FileViewModel> m_allFiles;
    private bool m_isRenaming;
    private string m_renameString;
    private bool m_isExpanded;
    private readonly SynchronizationContext m_synchronizationContext;

    public DirectoryViewModel(string path, ITreeNode parrent = null, IObservableCollection<FileViewModel> allFiles = null, SynchronizationContext synchronizationContext = null)
    {
      if (synchronizationContext == null)
      {
        if (parrent is DirectoryViewModel)
          synchronizationContext = ((DirectoryViewModel)parrent).m_synchronizationContext;
        if (synchronizationContext == null)
          synchronizationContext = SynchronizationContext.Current;
      }
      Debug.Assert(synchronizationContext != null, "SynchronizationContext should not be null");
      m_synchronizationContext = synchronizationContext;

      m_children = new SortedObservableCollection<ITreeNode>(new AsyncObservableCollection<ITreeNode>(synchronizationContext),
                          (node, treeNode) => (String.Compare(node.Name, treeNode.Name, StringComparison.Ordinal) + (node is DirectoryViewModel ? -1000 : 0) + (treeNode is DirectoryViewModel ? 1000 : 0)), m_synchronizationContext);
      m_path = path;
      m_parrent = parrent;
      m_name = System.IO.Path.GetFileName(path);
      if (!Directory.Exists(path))
        return;
      if (allFiles == null)
      {
        m_allFiles = new SortedObservableCollection<FileViewModel>(new AsyncObservableCollection<FileViewModel>(), (model, viewModel) => String.Compare(model.Name, viewModel.Name, StringComparison.Ordinal));
        allFiles = AllFiles;
      }
      foreach (string directory in Directory.GetDirectories(path))
        Children.Add(new DirectoryViewModel(directory, this, allFiles, m_synchronizationContext));
      foreach (string file in Directory.GetFiles(path))
      {
        FileViewModel fileViewModel = new FileViewModel(file, this);
        Children.Add(fileViewModel);
        allFiles.Add(fileViewModel);
      }
      if (parrent == null)
      {
        m_externalChanges = new Dictionary<string, DateTime>();
        m_fileSystemWatcher = new FileSystemWatcher(path)
        {
          NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
          IncludeSubdirectories = true
        };

        m_fileSystemWatcher.Created += FileSystemWatcherOnCreated;
        m_fileSystemWatcher.Deleted += FileSystemWatcherOnDeleted;
        m_fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
        m_fileSystemWatcher.Renamed += FileSystemWatcherOnRenamed;


        m_fileSystemWatcher.EnableRaisingEvents = true;
        m_cleanUpTimer = new Timer(ClanUp, null, 5000, Timeout.Infinite);
      }
    }

    public DirectoryViewModel CreateDirectory(string name)
    {
      if (Directory.Exists(Path + "\\" + name))
        return m_children.FirstOrDefault(n => n.Name == name) as DirectoryViewModel;

      DirectoryViewModel directoryViewModel = new DirectoryViewModel(Path + "\\" + name, this, m_allFiles, m_synchronizationContext);
      m_children.Add(directoryViewModel);
      try
      {
        Directory.CreateDirectory(Path + "\\" + name);
      }
      catch (Exception)
      {
        m_children.Remove(directoryViewModel);
        return null;
      }
      return directoryViewModel;
    }

    private void ClanUp(object state)
    {
      lock (m_externalChanges)
      {
        List<KeyValuePair<string, DateTime>> keyValuePairs = m_externalChanges.ToList();
        foreach (KeyValuePair<string, DateTime> keyValuePair in keyValuePairs)
        {
          if (keyValuePair.Value > DateTime.Now)
            m_externalChanges.Remove(keyValuePair.Key);
        }
      }

      m_cleanUpTimer.Change(5000, Timeout.Infinite);
    }

    private void FileSystemWatcherOnRenamed(object sender, RenamedEventArgs renamedEventArgs)
    {
      string[] path = renamedEventArgs.OldName.Split('\\');
      ITreeNode treeNode = this;
      foreach (string filename in path)
      {
        treeNode = treeNode.Children.First(n => n.Name == filename);
      }
      path = renamedEventArgs.Name.Split('\\');
      treeNode.Name = path[path.Length - 1];
    }

    private void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      lock (m_externalChanges)
      {
        if (m_externalChanges.ContainsKey(fileSystemEventArgs.FullPath))
          return;
        m_externalChanges.Add(fileSystemEventArgs.FullPath, DateTime.Now.AddSeconds(5));
      }
      string[] path = fileSystemEventArgs.Name.Split('\\');
      ITreeNode treeNode = this;
      foreach (string filename in path)
      {
        treeNode = treeNode.Children.First(n => n.Name == filename);
      }
      FileViewModel fileViewModel = treeNode as FileViewModel;
      if (fileViewModel != null)
        fileViewModel.RaisFileChangedOnDisk();
    }

    private void FileSystemWatcherOnDeleted(object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      string[] path = fileSystemEventArgs.Name.Split('\\');
      ITreeNode treeNode = this;
      for (int i = 0; i < path.Length - 1; i++)
        treeNode = treeNode.Children.First(n => n.Name == path[i]);
      ITreeNode currentTreeNode = treeNode.Children.FirstOrDefault(n => n.Name == path[path.Length - 1]);
      treeNode.Children.Remove(currentTreeNode);
      RemoveFromAllFiles(AllFiles, currentTreeNode);
    }

    private void RemoveFromAllFiles(IObservableCollection<FileViewModel> allFiles, ITreeNode treeNode)
    {
      FileViewModel fileViewModel = treeNode as FileViewModel;
      if (fileViewModel != null)
        allFiles.Remove(fileViewModel);
      else
      {
        DirectoryViewModel directoryViewModel = treeNode as DirectoryViewModel;
        if (directoryViewModel != null && directoryViewModel.Children != null)
        {
          foreach (ITreeNode child in directoryViewModel.Children)
          {
            RemoveFromAllFiles(allFiles, child);
          }
        }
      }
    }

    private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
    {
      string[] path = fileSystemEventArgs.Name.Split('\\');
      ITreeNode treeNode = this;
      for (int i = 0; i < path.Length - 1; i++)
      {
        treeNode = treeNode.Children.First(n => n.Name == path[i]);
      }
      if (treeNode.Children.Any(n => n.Path == fileSystemEventArgs.FullPath))
        return;
      if (File.Exists(fileSystemEventArgs.FullPath))
      {
        FileViewModel fileViewModel = new FileViewModel(fileSystemEventArgs.FullPath, treeNode);
        treeNode.Children.Add(fileViewModel);
        AllFiles.Add(fileViewModel);
      }
      else if (Directory.Exists(fileSystemEventArgs.FullPath))
        treeNode.Children.Add(new DirectoryViewModel(fileSystemEventArgs.FullPath, treeNode, AllFiles, m_synchronizationContext));
    }

    public bool HasUnsavedChanges
    {
      get { return false; }
      set {}
    }

    public bool IsExpanded
    {
      get { return m_isExpanded; }
      set
      {
        if (value.Equals(m_isExpanded)) return;
        m_isExpanded = value;
        OnPropertyChanged();
      }
    }

    public void Show()
    {
      ITreeNode node = m_parrent;
      while (node != null)
      {
        node.IsExpanded = true;
        node = node.Parent;
      }
    }

    public string Name
    {
      get { return m_name; }
      set
      {
        if (value == m_name) return;
        m_name = value;
        OnPropertyChanged();
      }
    }

    public string RenameString
    {
      get { return m_renameString; }
      set
      {
        if (value == m_renameString) return;
        m_renameString = value;
        OnPropertyChanged();
      }
    }

    public string Path { get { return m_parrent == null ? m_path : m_parrent.Path + "\\" + m_name; } }

    public bool IsRenaming
    {
      get { return m_isRenaming; }
      set
      {
        if (value.Equals(m_isRenaming)) return;
        m_isRenaming = value;
        if (m_isRenaming)
        {
          RenameString = m_name;
        }
        else
        {
          if (m_renameString != null && m_renameString != m_name)
          {
            try
            {
              string newPath = System.IO.Path.GetDirectoryName(Path) + "\\" + m_renameString;
              Directory.Move(Path, newPath);
              if (m_path != null)
                m_path = newPath;
              m_renameString = null;
            }
            catch (Exception)
            {
              Name = m_renameString;
            }
          }
        }
        OnPropertyChanged();
      }
    }

    public ITreeNode Parent { get { return m_parrent; } }

    public IObservableCollection<ITreeNode> Children
    {
      get { return m_children; }
    }

    public IFileViewModel GetFile(string path)
    {
      if (!path.StartsWith(m_path))
        return null;
      string relativPath = path.Substring(m_path.Length + 1);
      string[] splitpath = relativPath.Split('\\');
      ITreeNode treeNode = this;
      foreach (string s in splitpath)
      {
        treeNode = treeNode.Children.FirstOrDefault(n => n.Name == s);
        if (treeNode == null)
          return null;
      }
      return (IFileViewModel)treeNode;
    }

    public void Delete()
    {
      Directory.Delete(Path);
    }

    public string GetParameter(string key)
    {
      if (File.Exists(Path + "\\parameters.json"))
      {
        JsonException jsonException;
        JsonNode jsonNode = (JsonNode) JsonHelperFunctions.Parse(File.ReadAllText(Path + "\\parameters.json"), out jsonException);
        if (jsonException == null && jsonNode.ContainsKey(key))
          return jsonNode.GetObjectOrDefault(key, "");
      }
      return m_parrent == null ? null : m_parrent.GetParameter(key);
    }

    public IObservableCollection<FileViewModel> AllFiles
    {
      get { return m_allFiles; }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
      if (m_fileSystemWatcher != null)
      {
        m_fileSystemWatcher.Dispose();
        m_fileSystemWatcher = null;
      }
      if (m_cleanUpTimer != null)
      {
        m_cleanUpTimer.Dispose();
        m_cleanUpTimer = null;
      }
    }
  }
}
