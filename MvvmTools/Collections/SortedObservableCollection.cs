using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using SharpE.Definitions;
using SharpE.Definitions.Collection;
using SharpE.MvvmTools.Properties;

namespace SharpE.MvvmTools.Collections
{
  public class SortedObservableCollection<T> : IObservableCollection<T>
  {
    private IObservableCollection<T> m_baseObservableCollection;
    private readonly IComparer<T> m_comparer;
    private List<T> m_filteredList;
    private readonly SynchronizationContext m_synchronizationContext;
    private readonly object m_lockObject = new object();

    private class Comparer<T2> : IComparer<T2>
    {
      private readonly Func<T2, T2, int> m_comparer;

      public Comparer(Func<T2, T2, int> comparer)
      {
        m_comparer = comparer;
      }

      public int Compare(T2 x, T2 y)
      {
        return m_comparer(x, y);
      }
    }

    public SortedObservableCollection(IObservableCollection<T> baseObservableCollection, Func<T, T, int> comparer, SynchronizationContext synchronizationContext = null)
    {
      m_synchronizationContext = synchronizationContext ?? SynchronizationContext.Current;
      BindingOperations.EnableCollectionSynchronization(this, m_lockObject);

      m_comparer = new Comparer<T>(comparer);
      if (baseObservableCollection != null)
      {
        m_baseObservableCollection = baseObservableCollection;
        baseObservableCollection.CollectionChanged += BaseObservableCollectionOnCollectionChanged;
        m_filteredList = BaseObservableCollection.OrderBy(n => n, m_comparer).ToList();
      }
      else 
        m_filteredList = new List<T>();
    }

    private void BaseObservableCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      switch (notifyCollectionChangedEventArgs.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (T newItem in notifyCollectionChangedEventArgs.NewItems)
          {
            int index = m_filteredList.FindIndex(n => m_comparer.Compare(n, newItem) > 0);
            if (index == -1)
              m_filteredList.Add(newItem);
            else
              m_filteredList.Insert(index, newItem);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T> { newItem }, index));
          }
          OnPropertyChanged("Count");
          break;
        case NotifyCollectionChangedAction.Remove:
          int startingIndex = m_filteredList.IndexOf((T)notifyCollectionChangedEventArgs.OldItems[0]);
          foreach (T oldItem in notifyCollectionChangedEventArgs.OldItems)
            m_filteredList.Remove(oldItem);
          OnCollectionChanged(new  NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, notifyCollectionChangedEventArgs.OldItems, startingIndex));
          OnPropertyChanged("Count");
          break;
        case NotifyCollectionChangedAction.Replace:
          for (int i = 0; i < notifyCollectionChangedEventArgs.OldItems.Count; i++)
          {
            T oldItem = (T)notifyCollectionChangedEventArgs.OldItems[i];
            startingIndex = m_filteredList.IndexOf((T)notifyCollectionChangedEventArgs.OldItems[0]);
            m_filteredList.Remove(oldItem);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<T> { oldItem }, startingIndex));
            T newItem = (T)notifyCollectionChangedEventArgs.NewItems[i];
            int index = m_filteredList.FindIndex(n => m_comparer.Compare(n, newItem) > 0);
            if (index == -1)
              m_filteredList.Add(newItem);
            else
              m_filteredList.Insert(index, newItem);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T> { newItem }, index));
          }
          break;
        case NotifyCollectionChangedAction.Move:
          //Nothing need to happen, since position i detemined by sort.
          break;
        case NotifyCollectionChangedAction.Reset:
          m_filteredList = BaseObservableCollection.OrderBy(n => n, m_comparer).ToList();
          OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
          OnPropertyChanged("Count");
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public IEnumerator<T> GetEnumerator()
    {
      return m_filteredList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add(T item)
    {
      BaseObservableCollection.Add(item);
    }

    public void Clear()
    {
      foreach (T value in this.ToList())
        BaseObservableCollection.Remove(value);
    }

    public bool Contains(T item)
    {
      return m_filteredList.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      m_filteredList.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
      return BaseObservableCollection.Remove(item);
    }

    public int Count
    {
      get { return m_filteredList.Count; }
    }

    public bool IsReadOnly
    {
      get { return BaseObservableCollection.IsReadOnly; }
    }

    public IObservableCollection<T> BaseObservableCollection
    {
      get { return m_baseObservableCollection; }
      set
      {
        if (Equals(value, m_baseObservableCollection)) return;
        if (m_baseObservableCollection != null)
          m_baseObservableCollection.CollectionChanged -= BaseObservableCollectionOnCollectionChanged;
        m_baseObservableCollection = value;
        if (m_baseObservableCollection != null)
        {
          m_baseObservableCollection.CollectionChanged += BaseObservableCollectionOnCollectionChanged;
          m_filteredList = BaseObservableCollection.OrderBy(n => n, m_comparer).ToList();
        }
        else
        {
          m_filteredList = new List<T>();
        }
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        OnPropertyChanged("BaseObservableCollection");
        OnPropertyChanged("IsReadOnly");
      }
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged(string propertyName)
    {
      lock (this)
      {
        if (SynchronizationContext.Current == m_synchronizationContext)
        {
          RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        else
        {
          m_synchronizationContext.Send(RaisePropertyChanged, new PropertyChangedEventArgs(propertyName));
        }
      }
    }

    public int IndexOf(T item)
    {
      return m_filteredList.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      int baseIndex = BaseObservableCollection.IndexOf(m_filteredList[index]);
      BaseObservableCollection.Insert(baseIndex, item);
    }

    public void RemoveAt(int index)
    {
      BaseObservableCollection.Remove(m_filteredList[index]);
    }

    public T this[int index]
    {
      get { return m_filteredList[index]; }
      set
      {
        int baseIndex = BaseObservableCollection.IndexOf(m_filteredList[index]);
        BaseObservableCollection[baseIndex] = value;
      }
    }

    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      lock (this)
      {
        if (SynchronizationContext.Current == m_synchronizationContext)
        {
          RaiseCollectionChanged(e);
        }
        else
        {
          m_synchronizationContext.Send(RaiseCollectionChanged, e);
        }
      }
    }

    private void RaiseCollectionChanged(object param)
    {
        CollectionChanged(this, (NotifyCollectionChangedEventArgs)param);
    }

    private void RaisePropertyChanged(object param)
    {
      PropertyChanged(this, (PropertyChangedEventArgs)param);
    }
  }
}
