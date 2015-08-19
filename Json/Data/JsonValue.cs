namespace SharpE.Json.Data
{
  public class JsonValue : IDocPosition
  {
    private readonly object m_value;
    private readonly int m_lineIndex;
    private readonly int m_charIndex;

    public JsonValue(object value, int lineIndex, int charIndex)
    {
      m_lineIndex = lineIndex;
      m_value = value;
      m_charIndex = charIndex;
    }

    public int LineIndex
    {
      get { return m_lineIndex; }
    }

    public int CharIndex
    {
      get { return m_charIndex; }
    }

    public object Value
    {
      get { return m_value; }
    }

    public override string ToString()
    {
      return  m_value == null ? "null" : m_value.ToString();
    }

    public override bool Equals(object obj)
    {
      JsonValue jsonValue = obj as JsonValue;
      if (jsonValue != null)
        obj = jsonValue.Value;
      if (obj == null && m_value == null)
        return true;
      return obj != null && obj.Equals(m_value);
    }

    public override int GetHashCode()
    {
      return m_value == null ? 0 : m_value.GetHashCode();
    }

  }
}
