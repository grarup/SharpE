using System;
using SharpE.Definitions.Editor;

namespace SharpE.Definitions.Project
{
  public interface IFileViewModel : ITreeNode
  {
    event Func<bool> Saving;
    T GetContent<T>();
    void SetContent<T>(T content);
    string Exstension { get; }
    bool IsReadonly { get; }
    ValidationState ValidationState { get; set; }
    void Save();
    void SetTag(string key, object tag);
    object GetTag(string key);
    event Action<IFileViewModel> ContentChanged;
    event Action<IFileViewModel> FileChangedOnDisk;
    bool IsBasedOnCurrentFile();
    void Reset();
    void Reload();
  }
}
