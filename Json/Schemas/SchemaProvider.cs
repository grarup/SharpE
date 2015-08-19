using System.Collections.Generic;

namespace SharpE.Json.Schemas
{
  class SchemaProvider : ISchemaProvider
  {
    private readonly Dictionary<string, Schema> m_schemas = new Dictionary<string, Schema>();

    public void AddSchema(Schema schema)
    {
      m_schemas.Add(schema.Id, schema);
    }

    public Schema GetSchema(string path)
    {
      if (m_schemas.ContainsKey(path))
        return m_schemas[path];
      return null;
    }
  }
}