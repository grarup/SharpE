using System.ComponentModel;
using System.Windows.Input;
using SharpE.Definitions.Collection;

namespace SharpE.Definitions.Editor
{
  public interface IMenuItemViewModel : INotifyPropertyChanged
  {
    string Name { get; }
    IObservableCollection<IMenuItemViewModel> Children { get; }
    bool IsVisable { get; set; }
    object CommandParameter { get; set; }
    ICommand Command { get; }
  }
}