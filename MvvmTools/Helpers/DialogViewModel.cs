using System.ComponentModel;
using SharpE.Definitions;
using SharpE.Definitions.Collection;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Properties;

namespace SharpE.MvvmTools.Helpers
{
  public class DialogViewModel : INotifyPropertyChanged
  {
    private bool m_isShown;
    private readonly GenericManualCommand<IDialogViewModel> m_closeCommand;
    private bool m_isShowingCloseButton = true;
    private readonly AsyncObservableCollection<IDialogViewModel> m_viewModels = new AsyncObservableCollection<IDialogViewModel>();

    public DialogViewModel()
    {
      m_closeCommand = new GenericManualCommand<IDialogViewModel>(Close);
    }

    public void Close(IDialogViewModel viewModel)
    {
      if (!viewModel.CanClose()) return;
      ViewModels.Remove(viewModel);
      viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
      viewModel.IsShown = false;
      if (ViewModels.Count == 0)
        IsShown = false;
    }

    public bool IsShown
    {
      get { return m_isShown; }
      private set
      {
        if (value.Equals(m_isShown)) return;
        m_isShown = value;
        OnPropertyChanged("IsShown");
      }
    }

    public void Show(IDialogViewModel viewModel)
    {
      ViewModels.Add(viewModel);
      viewModel.IsShown = true;
      if (!viewModel.IsShown) return;
      viewModel.PropertyChanged += ViewModelOnPropertyChanged;
      IsShown = true;
    }

    private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      switch (propertyChangedEventArgs.PropertyName)
      {
        case "IsShown":
          if (!((IDialogViewModel)sender).IsShown)
            Close((IDialogViewModel)sender);
          break;
      }
    }

    public GenericManualCommand<IDialogViewModel> CloseCommand
    {
      get { return m_closeCommand; }
    }

    public bool IsShowingCloseButton
    {
      get { return m_isShowingCloseButton; }
      set
      {
        if (value.Equals(m_isShowingCloseButton)) return;
        m_isShowingCloseButton = value;
        OnPropertyChanged("IsShowingCloseButton");
      }
    }

    public IObservableCollection<IDialogViewModel> ViewModels
    {
      get { return m_viewModels; }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
