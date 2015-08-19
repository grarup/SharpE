using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SharpE.MvvmTools.Controls
{
  public class SimpleItemRender : IItemRender
  {
    private readonly Typeface m_typeface;

    public SimpleItemRender()
    {
      m_typeface = new Typeface(new FontFamily("Courier New"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
      FormattedText formattedText = new FormattedText("Peter", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_typeface, 14, Brushes.White);
      ItemHeight = formattedText.Height;
    }

    public double ItemHeight { get; private set; }

    public void Render(DrawingContext drawingContext, Point position, object item)
    {
      FormattedText formattedText = new FormattedText(item.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_typeface, 14, Brushes.White);
      drawingContext.DrawText(formattedText, position);
      
    }
  }
}
