using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Properties;
using SharpE.ViewModels.Tree;

namespace SharpE.Templats
{
  public class TemplateManager : INotifyPropertyChanged
  {
    private readonly IFileViewModel m_settings;
    private readonly List<string> m_paths = new List<string>();
    private readonly CollectionTreeViewModel m_root;
    private readonly IObservableCollection<Template> m_templates = new ObservableCollection<Template>();


    public TemplateManager(IFileViewModel settings)
    {
      m_settings = settings;
      m_settings.ContentChanged += SettingsOnContentChanged;
      m_root = new CollectionTreeViewModel();
      UpdateSettings();
    }

    private void SettingsOnContentChanged(IFileViewModel fileViewModel)
    {
      UpdateSettings();
    }

    public ITreeNode Root
    {
      get { return m_root; }
    }

    public IObservableCollection<Template> Templates
    {
      get { return m_templates; }
    }

    private void UpdateSettings()
    {
      m_paths.Clear();
      JsonNode settingsNode = (JsonNode)JsonHelperFunctions.Parse(m_settings.GetContent<string>());
      if (settingsNode == null) return;
      JsonArray jsonArray = settingsNode.GetObjectOrDefault<JsonArray>("templates", null);
      List<ITreeNode> nodes = m_root.Children.ToList();
      if (jsonArray != null)
      {
        foreach (JsonValue path in jsonArray)
        {
          m_paths.Add((string)path.Value);
          ITreeNode treeNode = m_root.Children.FirstOrDefault(n => n.Path == (string)path.Value);
          if (treeNode == null)
          {
            if (Directory.Exists((string) path.Value))
            {
              DirectoryViewModel directoryViewModel = new DirectoryViewModel((string)path.Value);
              directoryViewModel.AllFiles.CollectionChanged += AllFilesOnCollectionChanged;
              foreach (FileViewModel fileViewModel in directoryViewModel.AllFiles)
              {
                if (fileViewModel.Path.EndsWith("template.json"))
                  Templates.Add(new Template(fileViewModel));
              }
              m_root.Children.Add(directoryViewModel);
            }
          }
          else
            nodes.Remove(treeNode);
        }
      }
      foreach (ITreeNode treeNode in nodes)
        m_root.Children.Remove(treeNode);
    }

    private void AllFilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      switch (notifyCollectionChangedEventArgs.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (FileViewModel fileViewModel in notifyCollectionChangedEventArgs.NewItems)
          {
            if (fileViewModel.Path.EndsWith("template.json"))
              Templates.Add(new Template(fileViewModel));
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (FileViewModel fileViewModel in notifyCollectionChangedEventArgs.OldItems)
          {
            if (fileViewModel.Path.EndsWith("template.json"))
            {
              Template template = m_templates.FirstOrDefault(n => n.FileViewModel == fileViewModel);
              m_templates.Remove(template);
            }
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          break;
        case NotifyCollectionChangedAction.Move:
          break;
        case NotifyCollectionChangedAction.Reset:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
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
