using System;

namespace SharpE.Json.Data
{
  public class JsonException : ArgumentException
  {
    private readonly int m_charIndex;
    private readonly int m_lineIndex;

    public JsonException(string message, int lineIndex, int charIndex)
      : base(message + " - Line: " + lineIndex + ", Char: " + charIndex)
    {
      m_charIndex = charIndex;
      m_lineIndex = lineIndex;
    }

    public int CharIndex
    {
      get { return m_charIndex; }
    }

    public int LineIndex
    {
      get { return m_lineIndex; }
    }
  }
}
