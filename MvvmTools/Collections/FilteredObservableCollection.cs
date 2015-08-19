using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using SharpE.Definitions;
using SharpE.Definitions.Collection;
using SharpE.MvvmTools.Properties;

namespace SharpE.MvvmTools.Collections
{
  public class FilteredObservableCollection<T> : IObservableCollection<T>
  {
    private IObservableCollection<T> m_baseObservableCollection;
    private readonly Func<T, bool> m_filter;
    private List<T> m_filteredList;

    public FilteredObservableCollection(IObservableCollection<T> baseObservableCollection, Func<T, bool> filter)
    {
      m_baseObservableCollection = baseObservableCollection;
      if (m_baseObservableCollection != null)
        m_baseObservableCollection.CollectionChanged += BaseObservableCollectionOnCollectionChanged;
      m_filter = filter ?? (arg => true);
      Update();
    }

    private void BaseObservableCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      switch (notifyCollectionChangedEventArgs.Action)
      {
        case NotifyCollectionChangedAction.Add:
          List<T> newItems =
            notifyCollectionChangedEventArgs.NewItems.Cast<T>().Where(m_filter).ToList();
          if (newItems.Count == 0) return;
          m_filteredList.AddRange(newItems);
          CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, m_filteredList.IndexOf(newItems.First())));
          OnPropertyChanged("Count");
          break;
        case NotifyCollectionChangedAction.Remove:
          List<T> oldItems =
            notifyCollectionChangedEventArgs.OldItems.Cast<T>().Where(newItem => m_filter(newItem)).ToList();
          if (oldItems.Count == 0) return;
          int oldStartindex = m_filteredList.IndexOf(oldItems.First());
          foreach (T oldItem in oldItems)
            m_filteredList.Remove(oldItem);
          CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, oldStartindex));
          OnPropertyChanged("Count");
          break;
        case NotifyCollectionChangedAction.Replace:
          List<T> replacedItems = new List<T>();
          List<T> replacingItems = new List<T>();
          newItems = new List<T>();
          List<T> removedItems = new List<T>();
          for (int i = 0; i < notifyCollectionChangedEventArgs.OldItems.Count; i++)
          {
            T oldItem = (T) notifyCollectionChangedEventArgs.OldItems[i];
            T newItem = (T) notifyCollectionChangedEventArgs.NewItems[i];
            if (m_filter(oldItem))
            {
              if (m_filter(newItem))
              {
                replacedItems.Add(oldItem);
                replacingItems.Add(newItem);
                m_filteredList[m_filteredList.IndexOf(oldItem)] = newItem;
              }
              else
              {
                removedItems.Add(oldItem);
                m_filteredList.Remove(oldItem);
              }
            }
            else
            {
              if (m_filter(newItem))
              {
                newItems.Add(newItem);
                m_filteredList.Add(newItem);
              }
            }
          }
          if (replacingItems.Count > 0)
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, replacedItems, replacingItems));
          if (newItems.Count > 0)
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
          if (removedItems.Count > 0)
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
          break;
        case NotifyCollectionChangedAction.Move:
          //TODO implement
          Trace.WriteLine("Move is not implemented in FilterdObservableCollection");
          break;
        case NotifyCollectionChangedAction.Reset:
          m_filteredList = BaseObservableCollection.Where(m_filter).ToList();
          CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public void Update()
    {
      m_filteredList = m_baseObservableCollection == null ? new List<T>() : m_baseObservableCollection.Where(m_filter).ToList();
      CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      OnPropertyChanged("Count");
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
        m_baseObservableCollection.Remove(value);
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
      return m_filter(item) && m_baseObservableCollection.Remove(item);
    }

    public int Count
    {
      get { return m_filteredList.Count; }
    }

    public bool IsReadOnly
    {
      get { return m_baseObservableCollection.IsReadOnly; }
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
          m_baseObservableCollection.CollectionChanged += BaseObservableCollectionOnCollectionChanged;
        Update();
        OnPropertyChanged("BaseObservableCollection");
        OnPropertyChanged("IsReadOnly");
      }
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    public int IndexOf(T item)
    {
      return m_filteredList.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      int baseIndex = m_baseObservableCollection.IndexOf(m_filteredList[index]);
      m_baseObservableCollection.Insert(baseIndex, item);
    }

    public void RemoveAt(int index)
    {
      m_baseObservableCollection.Remove(m_filteredList[index]);
    }

    public T this[int index]
    {
      get { return m_filteredList[index]; }
      set
      {
        int baseIndex = m_baseObservableCollection.IndexOf(m_filteredList[index]);
        m_baseObservableCollection[baseIndex] = value;
      }
    }
  }
}
