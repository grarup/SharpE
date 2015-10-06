using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using SharpE.Definitions.Project;

namespace SharpE.Definitions.Editor
{
  public interface IEditor : INotifyPropertyChanged
  {
    string Name { get; }
    IFileViewModel Settings { get; }
    UIElement View { get; }
    IFileViewModel File { get; set; }
    IEnumerable<string> SupportedFiles { get; }
    IEditor CreateNew();
  }
}
