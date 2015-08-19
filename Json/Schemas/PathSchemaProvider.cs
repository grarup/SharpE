using System;
using System.IO;

namespace SharpE.Json.Schemas
{
  class PathSchemaProvider : ISchemaProvider
  {
    private readonly string m_schemaPath;
    private readonly SchemaManager m_schemaManager;

    public PathSchemaProvider(string schemaPath, SchemaManager schemaManager)
    {
      m_schemaPath = schemaPath;
      m_schemaManager = schemaManager;
    }

    public Schema GetSchema(string path)
    {
      if (!File.Exists(m_schemaPath + "\\" + path)) 
        return null;
      try
      {
        return new Schema(m_schemaPath + "\\" + path, m_schemaManager);
      }
      catch (Exception)
      {
        return null;
      }
    }
  }
}