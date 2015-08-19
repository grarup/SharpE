using SharpE.Json.Data;

namespace SharpE.Templats
{
  public class TemplateParameter
  {
    private readonly string m_name;
    private string m_value;
    private readonly TemplateParameterType m_type;
    private readonly bool m_isEditable;
    private readonly string m_key;
    private readonly string m_description;
    private readonly char m_pathSeparator;

    public TemplateParameter(string name, TemplateParameterType type = TemplateParameterType.Undefined)
    {
      m_name = name;
      m_type = type;
      m_pathSeparator = '\\';
    }

    public TemplateParameter(JsonNode jsonNode, Template template)
    {
      m_name = jsonNode.GetObjectOrDefault("name", "unknown");
      m_type = jsonNode.GetObjectOrDefault("type", TemplateParameterType.Undefined);
      m_value = jsonNode.GetObjectOrDefault("default", "");
      template.AddParameters(ref m_value, null, null);
      m_isEditable = !jsonNode.GetObjectOrDefault("isreadonly", false);
      m_description = jsonNode.GetObjectOrDefault<string>("description", null);
      m_key = jsonNode.GetObjectOrDefault<string>("key", null);
      string pathSeparator = jsonNode.GetObjectOrDefault("pathseparator", "\\");
      m_pathSeparator = pathSeparator.Length > 0 ? pathSeparator[0] : '\\';
      if (m_type == TemplateParameterType.File)
      {
        if (string.IsNullOrEmpty(m_value))
          m_value = template.TargetPath;
        m_value = m_value.Replace('\\', m_pathSeparator);
      }
    }

    public string Name
    {
      get { return m_name; }
    }

    public string Value
    {
      get { return m_value; }
      set { m_value = value; }
    }

    public string GetJsonValue(char? pathSeparator)
    {
      return m_type == TemplateParameterType.File ? m_value.Replace('\\', pathSeparator ?? m_pathSeparator).Replace("\\", "\\\\") : m_value;
    }

    public TemplateParameterType Type
    {
      get { return m_type; }
    }

    public bool IsEditable
    {
      get { return m_isEditable; }
    }

    public string Description
    {
      get { return m_description; }
    }

    public string Key
    {
      get { return m_key; }
    }

    public char PathSeparator
    {
      get { return m_pathSeparator; }
    }
  }
}
