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
  public class ConverterObservableCollection<T, T2> : IObservableCollection<T2>
  {
    private IObservableCollection<T> m_baseObservableCollection;
    private readonly Func<T, T2> m_converter;
    private bool m_autoDispose;
    private readonly List<KeyValuePair<T, T2>> m_newList = new List<KeyValuePair<T, T2>>();


    public ConverterObservableCollection(IObservableCollection<T> baseObservableCollection, Func<T, T2> converter, bool autoDispose = false)
    {
      m_converter = converter;
      AutoDispose = autoDispose;
      BaseObservableCollection = baseObservableCollection;
    }

    private void BaseObservableCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      switch (notifyCollectionChangedEventArgs.Action)
      {
        case NotifyCollectionChangedAction.Add:
          List<T> newItems =
            notifyCollectionChangedEventArgs.NewItems.Cast<T>().ToList();
          if (newItems.Count == 0) return;
          List<T2> newConvertedItems = new List<T2>();
          int insertIndex = notifyCollectionChangedEventArgs.NewStartingIndex;
          foreach (T newItem in newItems)
          {
            T2 newConvetedItem = m_converter(newItem);
            m_newList.Insert(insertIndex, new KeyValuePair<T, T2>(newItem, newConvetedItem));
            newConvertedItems.Add(newConvetedItem);
            insertIndex++;
          }
          CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newConvertedItems, notifyCollectionChangedEventArgs.NewStartingIndex));
          OnPropertyChanged("Count");
          break;
        case NotifyCollectionChangedAction.Remove:
          List<T> oldItems =
            notifyCollectionChangedEventArgs.OldItems.Cast<T>().ToList();
          if (oldItems.Count == 0) return;
          int oldStartIndex = m_newList.Select(n => n.Key).ToList().IndexOf(oldItems.First());
          List<T2> oldConvtedItems = oldItems.Select(n => m_newList.First(m => Equals(m.Key,n)).Value).ToList();
          foreach (T oldItem in oldItems)
            m_newList.Remove(m_newList.First(n => Equals(n.Key, oldItem)));
          CollectionChanged(this,  new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldConvtedItems,oldStartIndex));
          OnPropertyChanged("Count");
          break;
        case NotifyCollectionChangedAction.Replace:
          //TODO implement
          Trace.WriteLine("Replace is not implemented in FilterdObservableCollection");
          break;
        case NotifyCollectionChangedAction.Move:
          //TODO implement
          Trace.WriteLine("Move is not implemented in FilterdObservableCollection");
          break;
        case NotifyCollectionChangedAction.Reset:
          Update();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public IEnumerator<T2> GetEnumerator()
    {
      return m_newList.Select(n => n.Value).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    public void Add(T2 item)
    {
      throw new NotImplementedException();
    }

    public void Clear()
    {
      foreach (KeyValuePair<T, T2> keyValuePair in m_newList)
        m_baseObservableCollection.Remove(keyValuePair.Key);
    }

    public bool Contains(T2 item)
    {
      return m_newList.Any(n => Equals(n, item));
    }

    public void CopyTo(T2[] array, int arrayIndex)
    {
      m_newList.Select(n => n.Value).ToList().CopyTo(array, arrayIndex);
    }

    public bool Remove(T2 item)
    {
      return Contains(item) &&
             m_baseObservableCollection.Remove(m_newList.First(n => n.Value.Equals(item)).Key);
    }

    public int Count
    {
      get { return m_newList.Count; }
    }

    public bool IsReadOnly
    {
      get { return m_baseObservableCollection != null && m_baseObservableCollection.IsReadOnly; }
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
        }
        Update();
        OnPropertyChanged("BaseObservableCollection");
        OnPropertyChanged("IsReadOnly");
      }
    }

    public bool AutoDispose
    {
      get { return m_autoDispose; }
      set
      {
        if (value.Equals(m_autoDispose)) return;
        m_autoDispose = value;
        OnPropertyChanged("AutoDispose");
      }
    }

    public void Update()
    {
      List<T2> oldValues = m_newList.Select(n => n.Value).ToList();
      m_newList.Clear();
      if (m_baseObservableCollection != null)
      {
        foreach (T item in m_baseObservableCollection)
        m_newList.Add(new KeyValuePair<T, T2>(item, m_converter(item)));
      }
      CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      OnPropertyChanged("Count");
      if (m_autoDispose && oldValues.Any() && oldValues.First() is IDisposable)
      {
        foreach (T2 value in oldValues)
        {
          ((IDisposable)value).Dispose();
        }
      }
    }

    public int IndexOf(T2 item)
    {
      return m_newList.Select(n => n.Value).ToList().IndexOf(item);
    }

    public void Insert(int index, T2 item)
    {
      throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
      Remove(m_newList[index].Value);
    }

    public T2 this[int index]
    {
      get { return m_newList[index].Value; }
      set { throw new NotImplementedException(); }
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
