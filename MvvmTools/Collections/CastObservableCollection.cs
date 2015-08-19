using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SharpE.Definitions;
using SharpE.Definitions.Collection;
using SharpE.MvvmTools.Properties;

namespace SharpE.MvvmTools.Collections
{
  public class CastObservableCollection<T, T2> : IObservableCollection<T2> 
    where T : class
    where T2 : class
  {
    private readonly IObservableCollection<T> m_baseCollection;

    public CastObservableCollection(IObservableCollection<T> baseCollection)
    {
      m_baseCollection = baseCollection;
      m_baseCollection.CollectionChanged += BaseCollectionOnCollectionChanged;
      m_baseCollection.PropertyChanged += BaseCollectionOnPropertyChanged;
    }

    private void BaseCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      OnPropertyChanged(propertyChangedEventArgs.PropertyName);
    }

    private void BaseCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      CollectionChanged(this, notifyCollectionChangedEventArgs);
    }

    public IEnumerator<T2> GetEnumerator()
    {
      return m_baseCollection.Cast<T2>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add(T2 item)
    {
      if (item.GetType() == typeof (T))
      {
        m_baseCollection.Add(item as T);
      }
      else
      {
        throw new ArgumentException("item must be of type: " + typeof(T));
      }
    }

    public void Clear()
    {
      m_baseCollection.Clear();
    }

    public bool Contains(T2 item)
    {
      return m_baseCollection.Contains(item as T);
    }

    public void CopyTo(T2[] array, int arrayIndex)
    {
      m_baseCollection.Cast<T2>().ToList().CopyTo(array, arrayIndex);
    }

    public bool Remove(T2 item)
    {
      return m_baseCollection.Remove(item as T);
    }

    public int Count
    {
      get { return m_baseCollection.Count; }
    }

    public bool IsReadOnly
    {
      get { return m_baseCollection.IsReadOnly; }
    }

    public int IndexOf(T2 item)
    {
      return m_baseCollection.IndexOf(item as T);
    }

    public void Insert(int index, T2 item)
    {
      if (item.GetType() == typeof (T))
        m_baseCollection.Insert(index, item as T);
      else
        throw new ArgumentException("item must be of type: " + typeof (T));
    }

    public void RemoveAt(int index)
    {
      m_baseCollection.RemoveAt(index);
    }

    public T2 this[int index]
    {
      get { return m_baseCollection[index] as T2; }
      set { m_baseCollection[index] = value as T; }
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
