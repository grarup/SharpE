using System.ComponentModel;
using System.Runtime.CompilerServices;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Properties;

namespace SharpE.ViewModels.Dialogs
{
  class ReloadFileViewModel : INotifyPropertyChanged
  {
    private readonly IFileViewModel m_file;
    private bool m_isChecked = true;

    public ReloadFileViewModel(IFileViewModel file)
    {
      m_file = file;
    }

    public IFileViewModel File
    {
      get { return m_file; }
    }

    public bool IsChecked
    {
      get { return m_isChecked; }
      set
      {
        if (value.Equals(m_isChecked)) return;
        m_isChecked = value;
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
