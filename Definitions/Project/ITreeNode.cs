using System.ComponentModel;
using SharpE.Definitions.Collection;

namespace SharpE.Definitions.Project
{
  public interface ITreeNode : INotifyPropertyChanged
  {
    bool HasUnsavedChanges { get; set; }
    bool IsExpanded { get; set; }
    void Show();
    string Name { get; set; }
    string RenameString { get; set; }
    string Path { get; }
    bool IsRenaming { get; set; }
    ITreeNode Parent { get; }
    IObservableCollection<ITreeNode> Children { get; }
    IFileViewModel GetFile(string path);
    void Delete();
    string GetParameter(string key);
  }
}