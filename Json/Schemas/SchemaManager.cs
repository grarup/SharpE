using System.Collections.Generic;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Project;
using SharpE.Json.Data;
using SharpE.MvvmTools.Collections;

namespace SharpE.Json.Schemas
{
  public class SchemaManager
  {
    private readonly IFileViewModel m_settings;
    private readonly IObservableCollection<string> m_paths = new AsyncObservableCollection<string>();
    private readonly List<ISchemaProvider> m_schemaProviders = new List<ISchemaProvider>();
    private readonly SchemaProvider m_lockedSchemas = new SchemaProvider();

    public void AddLockedSchema(Schema schema)
    {
      m_lockedSchemas.AddSchema(schema);
    }

    public SchemaManager(IFileViewModel settings)
    {
      m_settings = settings;
      m_settings.FileChangedOnDisk += SettingsOnFileChangedOnDisk;
      AddLockedSchema(new Schema(Properties.Resources.schema_schema, this));
      AddLockedSchema(new Schema(Properties.Resources.quickschema_schema, this));
      UpdateSettings();
    }

    private void SettingsOnFileChangedOnDisk(IFileViewModel fileViewModel)
    {
      UpdateSettings();
    }

    private void UpdateSettings()
    {
      m_paths.Clear();
      m_schemaProviders.Clear();
      JsonNode settingsNode = (JsonNode) JsonHelperFunctions.Parse(m_settings.GetContent<string>());
      if (settingsNode == null)
        return;
      JsonArray jsonArray = settingsNode.GetObjectOrDefault<JsonArray>("schemas", null);
      if (jsonArray != null)
      {
        foreach (JsonValue path in jsonArray)
        {
          m_paths.Add((string) path.Value);
          m_schemaProviders.Add(new PathSchemaProvider((string) path.Value, this));
        }
      }
    }

    public IObservableCollection<string> Paths
    {
      get { return m_paths; }
    }

    public Schema GetSchema(string path)
    {
      Schema schema = m_lockedSchemas.GetSchema(path);
      if (schema != null)
        return schema;
      foreach (ISchemaProvider schemaProvider in m_schemaProviders)
      {
        schema = schemaProvider.GetSchema(path);
        if (schema != null)
          break;
      }
      return schema;
    }
  }
}
