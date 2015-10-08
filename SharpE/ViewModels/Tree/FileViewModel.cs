using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Properties;

namespace SharpE.ViewModels.Tree
{
  public class FileViewModel : IFileViewModel
  {
    private readonly ITreeNode m_parrent;
    private string m_name;
    private bool m_hasUnsavedChanges;
    private string m_path;
    private DateTime m_lastWriteTime;
    private readonly Dictionary<string, object> m_tags = new Dictionary<string, object>();
    private object m_content;
    private readonly bool m_isReadonly;
    private ValidationState m_validationState;
    private bool m_isRenaming;
    private string m_renameString;

    public event Func<bool> Saving;

    public FileViewModel(object content, string name)
    {
      m_content = content;
      m_name = name;
      m_isReadonly = true;
    }

    public FileViewModel(string path, ITreeNode parrent = null)
    {
      m_path = path;
      m_parrent = parrent;
      m_name = path == null ? "<unsaved>" : System.IO.Path.GetFileName(path);
    }

    public bool IsExpanded
    {
      get { return false; }
      set { }
    }

    public ITreeNode Parent { get { return m_parrent; } }

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

    public void Save()
    {
      if (!m_hasUnsavedChanges || m_isReadonly)
        return;
      if (m_parrent == null && m_path == null)
      {
        FileDialog fileDialog = new SaveFileDialog();
        bool? result = fileDialog.ShowDialog();
        if (result.Value)
        {
          m_name = System.IO.Path.GetFileName(fileDialog.FileName);
          m_path = fileDialog.FileName;
        }
        else
          return;
      }
      bool save = true;
      if (Saving != null)
        save = Saving();
      if (!save)
        return;

      HasUnsavedChanges = false;
      m_lastWriteTime = DateTime.Now;
      Stream stream = m_content as Stream;
      if (stream != null)
      {
        stream.Position = 0;
        Stream outStream = File.Create(Path,(int) stream.Length);
        byte[] data = new byte[stream.Length];
        stream.Read(data, 0, (int) stream.Length);
        outStream.Write(data,0,data.Length);
      }
      else
      {
        StreamWriter streamWriter = File.CreateText(Path);
        streamWriter.Write(m_content);
        streamWriter.Close();
      }
    }

    public void SetTag(string key, object tag)
    {
      if (m_tags.ContainsKey(key))
        m_tags[key] = tag;
      else
        m_tags.Add(key, tag);
    }

    public object GetTag(string key)
    {
      return !m_tags.ContainsKey(key) ? null : m_tags[key];
    }

    public bool IsBasedOnCurrentFile()
    {
      return m_lastWriteTime.Subtract(File.GetLastWriteTime(Path)).TotalMilliseconds > -100;
    }

    public T GetContent<T>()
    {
      if (m_content == null)
      {
        if (!File.Exists(Path))
          return default(T);
        if (typeof(T) == typeof(string))
        {
          StreamReader streamReader = File.OpenText(Path);
          m_content = streamReader.ReadToEnd();
          streamReader.Close();
          m_lastWriteTime = File.GetLastWriteTime(Path);
        }
        else if (typeof(T) == typeof(Stream))
        {
          m_content = new MemoryStream(File.ReadAllBytes(Path));
        }
        else if (typeof(T) == typeof(byte[]))
        {
          m_content = File.ReadAllBytes(Path);
        }
        else
        {
          throw new ArgumentException("Known types are string and stream");
        }
      }
      Stream stream = m_content as Stream;
      if (stream != null)
        stream.Position = 0;
      if (m_content is T)
        return (T)m_content;
      throw new ArgumentException("File have already been opened as " + m_content.GetType());
    }

    public void SetContent<T>(T content)
    {
      if (m_isReadonly) return;
      if (content.Equals(m_content)) return;
      m_content = content;
      HasUnsavedChanges = true;
      if (ContentChanged != null)
        ContentChanged(this);
    }

    public IObservableCollection<ITreeNode> Children
    {
      get { return null; }
    }

    public IFileViewModel GetFile(string path)
    {
      if (path == m_path)
        return this;
      return null;
    }

    public void Delete()
    {
      File.Delete(Path);
    }

    public string GetParameter(string key)
    {
      return m_parrent == null ? null : m_parrent.GetParameter(key);
    }

    public bool HasUnsavedChanges
    {
      get { return m_hasUnsavedChanges; }
      set
      {
        if (value.Equals(m_hasUnsavedChanges)) return;
        m_hasUnsavedChanges = value;
        OnPropertyChanged();
        OnPropertyChanged("Name");
      }
    }

    public string Path
    {
      get { return m_parrent == null ? m_path : m_parrent.Path + "\\" + m_name; }
    }

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
              File.Move(Path, newPath);
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

    public string Exstension
    {
      get { return m_name == "<unsaved>" ? null : System.IO.Path.GetExtension(m_name); }
    }

    public bool IsReadonly
    {
      get { return m_isReadonly; }
    }

    public ValidationState ValidationState
    {
      get { return m_validationState; }
      set
      {
        if (value == m_validationState) return;
        m_validationState = value;
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public event Action<IFileViewModel> FileChangedOnDisk;
    public event Action<IFileViewModel> ContentChanged;

    public void RaisFileChangedOnDisk()
    {
      Action<FileViewModel> action = FileChangedOnDisk;
      if (action != null) action(this);
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Reload()
    {
      m_content = null;
      if (ContentChanged != null)
        ContentChanged(this);
    }

    public void Reset()
    {
      m_content = null;
      HasUnsavedChanges = false;
    }

    public override string ToString()
    {
      return m_name;
    }
  }
}