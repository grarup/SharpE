using System.Windows.Media;

namespace SharpE.QubicleViewer
{
  public struct Qube
  {
    private readonly Color m_color;
    private readonly QubeSideMask m_mask;

    public Qube(uint value, uint colorFormat)
    {
      m_mask = (QubeSideMask) ((value >> 24) & 0xFF);
      if (colorFormat != 0)
        m_color = Color.FromRgb((byte) ((value >> 16) & 0xFF), (byte) ((value >> 8) & 0xFF), (byte) (value & 0xFF));
      else
        m_color = Color.FromRgb((byte)(value & 0xFF), (byte)((value >> 8) & 0xFF), (byte)((value >> 16) & 0xFF));
    }

    public Color Color
    {
      get { return m_color; }
    }

    public QubeSideMask Mask
    {
      get { return m_mask; }
    }
  }
}
