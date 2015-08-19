using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;

namespace SharpE.BaseEditors.AvalonTextEditorAddons
{
  public class TextBlock : ISegment
  {
    private readonly int m_offset;
    private readonly int m_endOffset;
    private readonly Color m_color;
    private readonly Color m_borderColor;

    public TextBlock(int offset, int endOffset, Color color)
    {
      m_color = color;
      m_borderColor = color;
      m_offset = offset;
      m_endOffset = endOffset;
    }
    
    public TextBlock(int offset, int endOffset, Color color, Color borderColor)
    {
      m_color = color;
      m_borderColor = borderColor;
      m_offset = offset;
      m_endOffset = endOffset;
    }

    public int Offset
    {
      get { return m_offset; }
    }

    public int Length
    {
      get { return m_endOffset - m_offset; }
    }

    public int EndOffset
    {
      get { return m_endOffset; }
    }

    public Color Color
    {
      get { return m_color; }
    }

    public Color BorderColor
    {
      get { return m_borderColor; }
    }
  }
}