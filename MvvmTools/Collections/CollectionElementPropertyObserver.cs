using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using SharpE.Definitions;
using SharpE.Definitions.Collection;

namespace SharpE.MvvmTools.Collections
{
  public class CollectionElementPropertyObserver<T> : IDisposable where T : INotifyPropertyChanged
  {
    private IObservableCollection<T> m_collection;
    private readonly PropertyChangedEventHandler m_handler;
    private List<T> m_elementListningTo = new List<T>();

    public CollectionElementPropertyObserver(IObservableCollection<T> collection, PropertyChangedEventHandler handler)
    {
      m_collection = collection;
      m_handler = handler;
      m_collection.CollectionChanged += CollectionOnCollectionChanged;
      m_elementListningTo.AddRange(m_collection);
      foreach (T element in m_elementListningTo)
      {
        element.PropertyChanged += handler;
      }
    }

    private void CollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      switch (notifyCollectionChangedEventArgs.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (T newItem in notifyCollectionChangedEventArgs.NewItems)
          {
            m_elementListningTo.Add(newItem);
            newItem.PropertyChanged += m_handler;
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (T oldItem in notifyCollectionChangedEventArgs.OldItems)
          {
            oldItem.PropertyChanged -= m_handler;
            m_elementListningTo.Remove(oldItem);
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          foreach (T oldItem in notifyCollectionChangedEventArgs.OldItems)
          {
            oldItem.PropertyChanged -= m_handler;
            m_elementListningTo.Remove(oldItem);
          }
          foreach (T newItem in notifyCollectionChangedEventArgs.NewItems)
          {
            m_elementListningTo.Add(newItem);
            newItem.PropertyChanged += m_handler;
          }
          break;
        case NotifyCollectionChangedAction.Move:
          break;
        case NotifyCollectionChangedAction.Reset:
          foreach (T element in m_elementListningTo)
          {
            element.PropertyChanged -= m_handler;
          }
          m_elementListningTo.Clear();
          if (m_collection == null) break;
          m_elementListningTo.AddRange(m_collection);
          foreach (T element in m_elementListningTo)
          {
            element.PropertyChanged += m_handler;
          }
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public void Dispose()
    {
      m_collection.CollectionChanged -= CollectionOnCollectionChanged;
      foreach (T element in m_elementListningTo)
      {
        element.PropertyChanged -= m_handler;
      }
      m_elementListningTo = null;
      m_collection = null;
    }
  }
}
