using System;
using System.Windows.Input;

namespace SharpE.MvvmTools.Commands
{
  public class AutoCommand : ICommand
  {
    private readonly Action m_executeAction;
    private readonly Func<bool> m_canExecute;

    public AutoCommand(Action executeAction, Func<bool> canExecute)
    {
      m_executeAction = executeAction;
      m_canExecute = canExecute;
    }

    #region ICommand Members
    public bool CanExecute(object parameter)
    {
      return m_canExecute();
    }

    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; } // This is evaluated everytime something happens on the UI
      remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameter)
    {
      m_executeAction();
    }
    #endregion
  }
}
