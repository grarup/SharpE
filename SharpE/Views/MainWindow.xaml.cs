using System.Windows;
using SharpE.ViewModels;

namespace SharpE.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      DataContext = new MainViewModel();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      Popup.IsOpen = !Popup.IsOpen;
    }
  }
}
