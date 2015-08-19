using System;
using System.Windows.Input;

namespace SharpE.MvvmTools.Commands
{
  public class GenericAutoCommand<T> : ICommand
  {
    private readonly Action<T> m_executeAction;
    private readonly Func<T, bool> m_canExecute;
    private readonly Func<object, T> m_converter;

    public GenericAutoCommand(Action<T> executeAction, Func<T, bool> canExecute = null, Func<object, T> converter = null)
    {
      m_executeAction = executeAction;
      m_canExecute = canExecute ?? (arg => true);
      m_converter = converter;
    }

    #region ICommand Members
    public bool CanExecute(object parameter)
    {
      if (parameter is T)
        return m_canExecute((T)parameter);
      return m_canExecute(m_converter == null ? default(T) : m_converter(parameter));
    }

    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; } // This is evaluated everytime something happens on the UI
      remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameter)
    {
      if (parameter is T)
        m_executeAction((T)parameter);
      else
        m_executeAction(m_converter == null ? default(T) : m_converter(parameter));
    }
    #endregion
  }
}
