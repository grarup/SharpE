using System.Collections.Generic;
using System.Linq;
using SharpE.Json.Data;

namespace SharpE.Json.Schemas
{
  public class Schema
  {
    private readonly string m_path;
    private readonly string m_name;
    private readonly SchemaObject m_root;
    private readonly SchemaManager m_schemaManager;
    private readonly JsonNode m_jsonNode;

    public Schema(byte[] data, SchemaManager schemaManager)
    {
      m_schemaManager = schemaManager;
      m_jsonNode = (JsonNode)JsonObject.Load(data);
      m_root = new SchemaObject(m_jsonNode, this);
      m_name = m_root.Id;
    }

    public Schema(string path, SchemaManager schemaManager)
    {
      m_path = path;
      m_schemaManager = schemaManager;
      m_jsonNode = (JsonNode)JsonObject.Load(path);
      m_name = System.IO.Path.GetFileName(path);
      m_root = new SchemaObject(m_jsonNode, this);
    }

    public string Name
    {
      get { return m_name; }
    }

    public string Path { get { return m_path; } }

    public SchemaObject GetSchemaObjectByRef(string path)
    {
      string[] array = path.Split('#');
      if (!string.IsNullOrEmpty(array[0]))
      {
        Schema schema = m_schemaManager.GetSchema(array[0]);
        if (schema == null) return null;
        return array.Length == 1 ? schema.m_root : schema.GetSchemaObjectByRef("#" + array[1]);
      }
      if (string.IsNullOrEmpty(array[1]))
        return m_root;
      string[] internalPath = array[1].Substring(1, array[1].Length - 1).Split('/');
      if (internalPath[0] == "definitions")
        return GetDefenition(internalPath);
      return GetSchemaObject(array[1].Substring(1, array[1].Length - 1).Split('/'));
    }

    public SchemaObject GetDefenition(IEnumerable<string> path)
    {
      IList<string> pathList = path.ToList();
      if (!m_root.Definitions.ContainsKey(pathList[1]))
        return null;
      return m_root.Definitions[pathList[1]];
    }

    public SchemaObject GetSchemaObject(IEnumerable<string> path)
    {
      IList<string> pathList = path.ToList();
      if (pathList.Count == 0)
        return m_root;
      int index = 0;
      SchemaObject schemaObject = m_root;
      while (pathList.Count > index && schemaObject != null)
      {
        if (schemaObject.Type == SchemaDataType.Object)
          schemaObject = schemaObject.GetChild(pathList[index]);
        else if (schemaObject.Type == SchemaDataType.Array)
        {
          if (schemaObject.Items != null)
            schemaObject = schemaObject.Items[0];
          else
            return null;
        }
        index++;
      }
      return schemaObject;
    }

    public string Id
    {
      get
      {
        return m_root != null ? m_root.Id : "";
      }
    }

    public string Text { get { return m_jsonNode.ToString(); } }

    public bool Validate(object obj, string loaclPath)
    {
      Dictionary<int, List<ValidationError>> errors = new Dictionary<int, List<ValidationError>>();
      return Validate(obj, errors, loaclPath) && errors.Count == 0;
    }

    public bool Validate(object obj, Dictionary<int, List<ValidationError>> errors, string localPath)
    {
      return m_root.Validate(obj, errors, localPath);
    }
  }
}
