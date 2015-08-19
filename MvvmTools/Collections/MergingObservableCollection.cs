using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SharpE.Definitions;
using SharpE.Definitions.Collection;

namespace SharpE.MvvmTools.Collections
{
  public class MergingObservableCollection<T> : AsyncObservableCollection<T>
  {
    private IObservableCollection<T> m_primaryObservableCollection;
    private IObservableCollection<T> m_secondaryObservableCollection;
    private bool m_addDuplicatesTwice;

    public MergingObservableCollection(bool addDuplicatesTwice = true)
    {
      m_addDuplicatesTwice = addDuplicatesTwice;
    }

    public MergingObservableCollection(IObservableCollection<T> primaryObservableCollection, IObservableCollection<T> secondaryObservableCollection, bool addDuplicatesTwice = true)
    {
      m_primaryObservableCollection = primaryObservableCollection;
      m_secondaryObservableCollection = secondaryObservableCollection;
      AddDuplicatesTwice = addDuplicatesTwice;

      if (m_primaryObservableCollection != null) 
        m_primaryObservableCollection.CollectionChanged += PrimaryObservableCollectionOnCollectionChanged;
      if (m_secondaryObservableCollection != null)
        m_secondaryObservableCollection.CollectionChanged += SecondaryObservableCollectionOnCollectionChanged;
    }

    public bool IsReadOnly { get { return true; } }

    private void SecondaryObservableCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
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
            if (AddDuplicatesTwice || !PrimaryObservableCollection.Contains(oldItem))
              Remove(oldItem);
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          foreach (T oldItem in notifyCollectionChangedEventArgs.OldItems)
          {
            if (AddDuplicatesTwice || !PrimaryObservableCollection.Contains(oldItem))
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

    public IObservableCollection<T> PrimaryObservableCollection
    {
      get { return m_primaryObservableCollection; }
      set
      {
        if (m_primaryObservableCollection == value) return;
        if (m_primaryObservableCollection != null)
          m_primaryObservableCollection.CollectionChanged -= PrimaryObservableCollectionOnCollectionChanged;
        m_primaryObservableCollection = value;
        ResetCollection();
        if (m_primaryObservableCollection != null)
          m_primaryObservableCollection.CollectionChanged += PrimaryObservableCollectionOnCollectionChanged;
        OnPropertyChanged(new PropertyChangedEventArgs("PrimaryObservableCollection"));
      }
    }

    public IObservableCollection<T> SecondaryObservableCollection
    {
      get { return m_secondaryObservableCollection; }
      set
      {
        if (m_secondaryObservableCollection == value) return;
        if (m_secondaryObservableCollection != null)
          m_secondaryObservableCollection.CollectionChanged -= SecondaryObservableCollectionOnCollectionChanged;
        m_secondaryObservableCollection = value;
        ResetCollection();
        if (m_secondaryObservableCollection != null)
          m_secondaryObservableCollection.CollectionChanged += SecondaryObservableCollectionOnCollectionChanged;
        OnPropertyChanged(new PropertyChangedEventArgs("SecondaryObservableCollection"));
      }
    }

    private void ResetCollection()
    {
      Items.Clear();
      if (m_primaryObservableCollection != null)
        foreach (T item in PrimaryObservableCollection)
          Items.Add(item);
      if (m_secondaryObservableCollection != null)
        foreach (T item in SecondaryObservableCollection.Where(item => AddDuplicatesTwice || !Items.Contains(item)))
          Items.Add(item);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void PrimaryObservableCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
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
            if (AddDuplicatesTwice || !SecondaryObservableCollection.Contains(oldItem))
              Remove(oldItem);
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          foreach (T oldItem in notifyCollectionChangedEventArgs.OldItems)
          {
            if (AddDuplicatesTwice || !SecondaryObservableCollection.Contains(oldItem))
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
  }
}
