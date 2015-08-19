using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SharpE.Views.Dialogs
{
  /// <summary>
  /// Interaction logic for FileSwitchView.xaml
  /// </summary>
  public partial class FileSwitchView
  {
    public FileSwitchView()
    {
      InitializeComponent();
    }

    private void FileSwitchView_OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (IsVisible)
      {
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate
        {
          Focus();
          Keyboard.Focus(this);
        }));
      }
    }

  }
}
