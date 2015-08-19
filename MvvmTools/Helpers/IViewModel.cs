using System.ComponentModel;
using System.Windows;

namespace SharpE.MvvmTools.Helpers
{
  public interface IViewModel : INotifyPropertyChanged
  {
    FrameworkElement View { get; }
  }
}
