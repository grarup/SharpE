using System;
using SharpE.MvvmTools.Commands;

namespace SharpE.MvvmTools.Helpers
{
  public class BaseDialogViewModel : BaseViewModel, IDialogViewModel
  {
    private IOwnerViewModel m_ownerViewModel;
    protected Func<bool> m_canClose = () => true;
    protected bool m_showCloseButton = true;
    private bool m_isShown;
    private readonly ManualCommand m_closeCommand;

    public BaseDialogViewModel()
    {
      m_closeCommand = new ManualCommand(() => IsShown = false);
    }

    public IOwnerViewModel OwnerViewModel
    {
      get { return m_ownerViewModel; }
      set { m_ownerViewModel = value; }
    }

    public Func<bool> CanClose
    {
      get { return m_canClose; }
    }

    public bool ShowCloseButton
    {
      get { return m_showCloseButton; }
    }

    public ManualCommand CloseCommand
    {
      get { return m_closeCommand; }
    }

    public virtual bool IsShown
    {
      get { return m_isShown; }
      set
      {
        if (value.Equals(m_isShown)) return;
        m_isShown = value;
        OnPropertyChanged();
      }
    }
  }
}
