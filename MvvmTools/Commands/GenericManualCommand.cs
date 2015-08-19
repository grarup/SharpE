using System;
using System.Threading;
using System.Windows.Input;

namespace SharpE.MvvmTools.Commands
{
  public class GenericManualCommand<T> : ICommand
  {
    private readonly Action<T> m_executeAction;
    private readonly Func<object, T> m_converter;
    private readonly Func<T, bool> m_canExecute;
    private readonly SynchronizationContext m_synchronizationContext;


    public GenericManualCommand(Action<T> executeAction, Func<T, bool> canExecute = null , Func<object, T> converter = null, SynchronizationContext synchronizationContext = null)
    {
      m_executeAction = executeAction;
      m_converter = converter;
      m_canExecute = canExecute ?? (arg => true);
      m_synchronizationContext = m_synchronizationContext == null
                                   ? SynchronizationContext.Current
                                   : synchronizationContext;
    }
    
    public bool CanExecute(object parameter)
    {
      if (parameter is T)
        return m_canExecute((T) parameter);
      return m_canExecute(m_converter != null ? m_converter(parameter) : default(T));
    }

    public event EventHandler CanExecuteChanged = delegate {};

    public void Execute(object parameter)
    {
      if (parameter is T)
        m_executeAction((T) parameter);
      else
        m_executeAction(m_converter != null ? m_converter(parameter) : default(T));
    }

    public void Update()
    {
      if (m_synchronizationContext == SynchronizationContext.Current)
        FireCanExecuteChanged(null);
      else
        m_synchronizationContext.Send(FireCanExecuteChanged, null);
    }

    private void FireCanExecuteChanged(object state)
    {
      CanExecuteChanged(this, new EventArgs());
    }
  }
}
