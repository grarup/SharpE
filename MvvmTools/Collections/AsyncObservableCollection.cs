using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows.Data;
using SharpE.Definitions;
using SharpE.Definitions.Collection;
using SharpE.MvvmTools.Properties;

namespace SharpE.MvvmTools.Collections
{
  public class AsyncObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>, IObservableCollection<T>
  {
    private readonly SynchronizationContext m_synchronizationContext;
    private readonly object m_lockObject = new object();

    public AsyncObservableCollection()
    {
      m_synchronizationContext = SynchronizationContext.Current;
      BindingOperations.EnableCollectionSynchronization(this, m_lockObject);
    }

    public AsyncObservableCollection(SynchronizationContext synchronizationContext = null)
    {
      m_synchronizationContext = synchronizationContext ?? SynchronizationContext.Current;
      BindingOperations.EnableCollectionSynchronization(this, m_lockObject);
    }

    public AsyncObservableCollection(IEnumerable<T> list, SynchronizationContext synchronizationContext = null)
      : base(list)
    {
      m_synchronizationContext = synchronizationContext ?? SynchronizationContext.Current;
      BindingOperations.EnableCollectionSynchronization(this, m_lockObject);
    }

    public void AddRange(IEnumerable<T> collection)
    {
      foreach (T obj in collection)
        Items.Add(obj);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
    }
    
    public SynchronizationContext SynchronizationContext
    {
      get { return m_synchronizationContext; }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
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
      base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
    }

    [NotifyPropertyChangedInvocator]
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      lock (this)
      {
        if (SynchronizationContext.Current == m_synchronizationContext)
        {
          RaisePropertyChanged(e);
        }
        else
        {
          m_synchronizationContext.Send(RaisePropertyChanged, e);
        }
      }
    }

    private void RaisePropertyChanged(object param)
    {
      base.OnPropertyChanged((PropertyChangedEventArgs)param);
    }
  }
}
