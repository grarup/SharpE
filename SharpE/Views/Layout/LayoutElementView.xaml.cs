using System.Windows;

namespace SharpE.Views.Layout
{
  /// <summary>
  /// Interaction logic for EditorLayoutView.xaml
  /// </summary>
  public partial class LayoutElementView
  {
    public LayoutElementView()
    {
      InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      Popup.IsOpen = !Popup.IsOpen;
    }
  }
}
