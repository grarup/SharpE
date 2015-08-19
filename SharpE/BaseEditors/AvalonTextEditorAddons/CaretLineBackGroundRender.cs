using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace SharpE.BaseEditors.AvalonTextEditorAddons
{
  public class CaretLineBackGroundRender : IBackgroundRenderer
  {
    private readonly ICSharpCode.AvalonEdit.TextEditor m_textEditor;

    public CaretLineBackGroundRender(ICSharpCode.AvalonEdit.TextEditor textEditor)
    {
      m_textEditor = textEditor;
      m_textEditor.TextArea.Caret.PositionChanged += (sender, e) =>
        m_textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
      if (m_textEditor.Document == null)
        return;

      textView.EnsureVisualLines();

      var currentLine = m_textEditor.Document.GetLineByOffset(m_textEditor.CaretOffset);
      foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
      {
        drawingContext.DrawRectangle(
          new SolidColorBrush(Color.FromArgb(0xFF, 0x10, 0x10, 0x10)), null,
          new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
        Brush edgeBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x60, 0x60, 0x60));
        drawingContext.DrawRectangle(
          edgeBrush, null,
          new Rect(rect.Location, new Size(textView.ActualWidth, 1)));
        Point point = rect.Location;
        point.Offset(0, rect.Height);
        drawingContext.DrawRectangle(
          edgeBrush, null,
          new Rect(point, new Size(textView.ActualWidth, 1)));
      }
    }

    public KnownLayer Layer
    {
      get { return KnownLayer.Background; }
    }
  }
}