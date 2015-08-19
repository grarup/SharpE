using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using SharpE.MvvmTools.Properties;

namespace SharpE.MvvmTools.Helpers
{
  public class BaseViewModel : IViewModel
  {
    protected FrameworkElement m_view;
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual FrameworkElement View
    {
      get { return m_view ?? (m_view = GenerateView()); }
    }

    protected virtual FrameworkElement GenerateView()
    {
      return new TextBlock { Text = "Not yet implemented", FontSize = 24, Padding = new Thickness(10) }; 
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
