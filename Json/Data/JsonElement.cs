namespace SharpE.Json.Data
{
  public class JsonElement : IDocPosition
  {
    private readonly string m_key;
    private object m_value;
    private readonly int m_lineIndex;
    private readonly int m_charIndex;

    public JsonElement(string key, object value)
    {
      m_key = key;
      m_value = value;
      m_lineIndex = -1;
      m_charIndex = -1;
    }

    public JsonElement(string key, object value, int lineIndex, int charIndex)
    {
      m_key = key;
      m_value = value;
      m_lineIndex = lineIndex;
      m_charIndex = charIndex;
    }

    public string Key
    {
      get { return m_key; }
    }

    public object Value
    {
      get { return m_value; }
      set { m_value = value; }
    }

    public int LineIndex
    {
      get { return m_lineIndex; }
    }

    public int CharIndex
    {
      get { return m_charIndex; }
    }
  }
}
