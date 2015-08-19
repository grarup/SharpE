using SharpE.Json.Schemas;

namespace SharpE.Json.AutoComplet
{
  public struct AutoCompleteValue
  {
    private readonly AutocompleteType m_type;
    private readonly object m_value;
    private readonly SchemaObject m_schemaObject;

    public AutoCompleteValue(AutocompleteType type, object value, SchemaObject schemaObject)
    {
      m_type = type;
      m_value = value;
      m_schemaObject = schemaObject;
    }

    public AutocompleteType Type
    {
      get { return m_type; }
    }

    public object Value
    {
      get { return m_value; }
    }

    public SchemaObject SchemaObject
    {
      get { return m_schemaObject; }
    }
  }
}
