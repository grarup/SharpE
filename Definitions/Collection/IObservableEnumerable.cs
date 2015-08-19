using System.Collections.Generic;
using System.Collections.Specialized;

namespace SharpE.Definitions.Collection
{
  public interface IObservableEnumerable<out T> : IEnumerable<T>, INotifyCollectionChanged
  {
  }
}
