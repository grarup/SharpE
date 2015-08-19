using System;
using System.Threading;
using System.Windows.Input;

namespace SharpE.MvvmTools.Commands
{
  public class ManualCommand : ICommand
  {
    private readonly Action m_executeAction;
    private readonly Func<bool> m_canExecute;
    private readonly SynchronizationContext m_synchronizationContext;

    public ManualCommand(Action executeAction, SynchronizationContext synchronizationContext = null)
      : this(executeAction, () => true, synchronizationContext)
    {
    }

    public ManualCommand(Action executeAction, Func<bool> canExecute,
                         SynchronizationContext synchronizationContext = null)
    {
      m_executeAction = executeAction;
      m_canExecute = canExecute;
      m_synchronizationContext = synchronizationContext ?? SynchronizationContext.Current;
    }

    public bool CanExecute(object parameter)
    {
      return m_canExecute();
    }

    public event EventHandler CanExecuteChanged = delegate { };

    public void Execute(object parameter)
    {
      if (m_executeAction != null)
        m_executeAction();
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
