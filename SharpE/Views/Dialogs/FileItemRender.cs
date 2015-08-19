using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using SharpE.MvvmTools.Controls;
using SharpE.ViewModels.Tree;

namespace SharpE.Views.Dialogs
{
  public class FileItemRender : FrameworkElement, IItemRender
  {
    private readonly Typeface m_typeface;

    public static readonly DependencyProperty ShortenPathProperty = DependencyProperty.Register(
      "ShortenPath", typeof (int), typeof (FileItemRender), new PropertyMetadata(default(int), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      FileItemRender fileItemRender = dependencyObject as FileItemRender;
      if (fileItemRender == null) return;
      fileItemRender.InvalidateVisual();
    }

    public int ShortenPath
    {
      get { return (int) GetValue(ShortenPathProperty); }
      set { SetValue(ShortenPathProperty, value); }
    }

    public FileItemRender()
    {
      m_typeface = new Typeface(new FontFamily("Courier New"), FontStyles.Normal, FontWeights.Normal,
        FontStretches.Normal);
      FormattedText formattedText = new FormattedText("Peter", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
        m_typeface, 14, Brushes.White);
      FormattedText formattedTextSmall = new FormattedText("Peter", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
        m_typeface, 10, Brushes.Wheat);
      ItemHeight = formattedText.Height + formattedTextSmall.Height;
    }

    public double ItemHeight { get; private set; }

    public void Render(DrawingContext drawingContext, Point position, object item)
    {
      FileViewModel fileViewModel = item as FileViewModel;
      if (fileViewModel == null) return;
      FormattedText formattedText = new FormattedText(fileViewModel.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
        m_typeface, 14, Brushes.White);
      drawingContext.DrawText(formattedText, position);
      string path = Path.GetDirectoryName(fileViewModel.Path) ?? "";
      path = path.Length > ShortenPath ? path.Substring(ShortenPath, path.Length - ShortenPath) : "\\";
      FormattedText formattedTextSmall = new FormattedText(path, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
        m_typeface, 10, Brushes.LightSlateGray);
      drawingContext.DrawText(formattedTextSmall, new Point(position.X + 5, position.Y + formattedText.Height));

    }
  }
}
