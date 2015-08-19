using System.Collections.Generic;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace SharpE.BaseEditors.AvalonTextEditorAddons
{
  public class TextMarkingBackGroundRender : IBackgroundRenderer
  {
    private readonly ICSharpCode.AvalonEdit.TextEditor m_textEditor;
    private List<TextBlock> m_textBlocks;

    public TextMarkingBackGroundRender(ICSharpCode.AvalonEdit.TextEditor textEditor)
    {
      m_textEditor = textEditor;
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
      if (m_textEditor.Document == null || m_textBlocks == null)
        return;

      textView.EnsureVisualLines();

      foreach (TextBlock textBlock in m_textBlocks)
      {
        foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, textBlock))
        {
          drawingContext.DrawRectangle(
            new SolidColorBrush(textBlock.Color), new Pen(new SolidColorBrush(textBlock.BorderColor), 1), 
            rect);
        }
      }
    }

    public KnownLayer Layer
    {
      get { return KnownLayer.Background; }
    }

    public List<TextBlock> TextBlocks
    {
      get { return m_textBlocks; }
      set
      {
        m_textBlocks = value;
        if (m_textEditor.Dispatcher.CheckAccess())
          m_textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        else
          m_textEditor.Dispatcher.Invoke(() => m_textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background));
      }
    }
  }
}
