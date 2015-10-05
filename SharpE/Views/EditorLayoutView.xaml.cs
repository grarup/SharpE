using System.Windows;
using System.Windows.Controls;

namespace SharpE.Views
{
  /// <summary>
  /// Interaction logic for EditorLayoutView.xaml
  /// </summary>
  public partial class EditorLayoutView
  {
    public EditorLayoutView()
    {
      InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      Popup.IsOpen = !Popup.IsOpen;
    }
  }
}
