using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using SharpE.Definitions;
using SharpE.Definitions.Collection;

namespace SharpE.MvvmTools.Collections
{
  public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>, IObservableCollection<T>
  {
    public void AddRange(IEnumerable<T> collection)
    {
      int startIndex = Items.Count;
      foreach (T obj in collection)
        Items.Add(obj);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection, startIndex));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
    }
  }
}
