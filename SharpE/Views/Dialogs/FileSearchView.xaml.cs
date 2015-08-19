using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SharpE.Views.Dialogs
{
  /// <summary>
  /// Interaction logic for FolderBrowserView.xaml
  /// </summary>
  public partial class FileSearchView
  {
    public FileSearchView()
    {
      InitializeComponent();
    }

    private void FileSearchView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (IsVisible)
      {
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate
          {
            SearchBox.Focus();
            Keyboard.Focus(SearchBox);
          }));
      }
    }
  }
}
