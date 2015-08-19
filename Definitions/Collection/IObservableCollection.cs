using System.Collections.Generic;
using System.ComponentModel;

namespace SharpE.Definitions.Collection
{
  public interface IObservableCollection<T> : IList<T>, IObservableEnumerable<T>, INotifyPropertyChanged
  {
  }
}
