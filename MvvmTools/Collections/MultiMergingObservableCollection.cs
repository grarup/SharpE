using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SharpE.Definitions;
using SharpE.Definitions.Collection;

namespace SharpE.MvvmTools.Collections
{
  public class MultiMergingObservableCollection<T> : AsyncObservableCollection<T>
  {
    private readonly List<IObservableCollection<T>> m_observableCollections;
    private bool m_addDuplicatesTwice;

    public MultiMergingObservableCollection(bool addDuplicatesTwice = true)
    {
      m_addDuplicatesTwice = addDuplicatesTwice;
    }

    public MultiMergingObservableCollection(List<IObservableCollection<T>> observableCollections,
                                            bool addDuplicatesTwice = true)
    {
      m_observableCollections = observableCollections;
      m_addDuplicatesTwice = addDuplicatesTwice;

      foreach (IObservableCollection<T> observableCollection in m_observableCollections)
      {
        observableCollection.CollectionChanged += SecondaryObservableCollectionOnCollectionChanged;
        foreach (T item in observableCollection.Where(item => AddDuplicatesTwice || !Items.Contains(item)))
          Items.Add(item);
      }
    }

    public bool IsReadOnly
    {
      get { return true; }
    }

    private void SecondaryObservableCollectionOnCollectionChanged(object sender,
                                                                  NotifyCollectionChangedEventArgs
                                                                    notifyCollectionChangedEventArgs)
    {
      switch (notifyCollectionChangedEventArgs.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (T newItem in notifyCollectionChangedEventArgs.NewItems)
          {
            if (AddDuplicatesTwice || !Contains(newItem))
              Add(newItem);
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (T oldItem in notifyCollectionChangedEventArgs.OldItems)
          {
            if (AddDuplicatesTwice || !m_observableCollections.Any(n => n.Contains(oldItem)))
              Remove(oldItem);
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          foreach (T oldItem in notifyCollectionChangedEventArgs.OldItems)
          {
            if (AddDuplicatesTwice || !m_observableCollections.Any(n => n.Contains(oldItem)))
              Remove(oldItem);
          }
          foreach (T newItem in notifyCollectionChangedEventArgs.NewItems)
          {
            if (AddDuplicatesTwice || !Contains(newItem))
              Add(newItem);
          }
          break;
        case NotifyCollectionChangedAction.Move:
          break;
        case NotifyCollectionChangedAction.Reset:
          ResetCollection();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public bool AddDuplicatesTwice
    {
      get { return m_addDuplicatesTwice; }
      set
      {
        m_addDuplicatesTwice = value;
        ResetCollection();
      }
    }

    private void ResetCollection()
    {
      Items.Clear();
      foreach (IObservableCollection<T> observableCollection in m_observableCollections)
      {
        foreach (T item in observableCollection.Where(item => AddDuplicatesTwice || !Items.Contains(item)))
          Items.Add(item);
      }
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
  }
}
