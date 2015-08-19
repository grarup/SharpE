using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Properties;

namespace SharpE.ViewModels.Tree
{
  class CollectionTreeViewModel : ITreeNode
  {
    private readonly IObservableCollection<string> m_paths;
    private readonly IObservableCollection<ITreeNode> m_children = new AsyncObservableCollection<ITreeNode>();
    private bool m_isExpanded;

    public CollectionTreeViewModel(IObservableCollection<string> paths = null)
    {
      m_paths = paths;
      if (paths != null)
      {
        foreach (string path in paths)
        {
          if (Directory.Exists(path))
            m_children.Add(new DirectoryViewModel(path));
        }
        paths.CollectionChanged += PathsOnCollectionChanged;
      }
    }

    private void PathsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      switch (notifyCollectionChangedEventArgs.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (string newItem in notifyCollectionChangedEventArgs.NewItems)
          {
            if (Directory.Exists(newItem))
              m_children.Add(new DirectoryViewModel(newItem));
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (string oldItem in notifyCollectionChangedEventArgs.OldItems)
          {
            ITreeNode treeNode = m_children.FirstOrDefault(n => n.Path == oldItem);
            if (treeNode != null)
              m_children.Remove(treeNode);
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          break;
        case NotifyCollectionChangedAction.Move:
          break;
        case NotifyCollectionChangedAction.Reset:
          m_children.Clear();
          foreach (string path in m_paths)
          {
            if (Directory.Exists(path))
              m_children.Add(new DirectoryViewModel(path));
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
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
      m_isExpanded = true;
    }

    public string Name
    {
      get { return ""; }
      set {}
    }

    public string RenameString
    {
      get { return ""; }
      set {  }
    }

    public string Path
    {
      get { return null; } 
      
    }

    public bool IsRenaming
    {
      get { return false; }
      set {  }
    }

    public ITreeNode Parent { get { return null; } }

    public IObservableCollection<ITreeNode> Children
    {
      get { return m_children; }
    }

    public IFileViewModel GetFile(string path)
    {
      foreach (ITreeNode treeNode in m_children)
      {
        IFileViewModel fileViewModel = treeNode.GetFile(path);
        if (fileViewModel != null)
          return fileViewModel;
      }
      return null;
    }

    public void Delete()
    {
      
    }

    public string GetParameter(string key)
    {
      return null;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
