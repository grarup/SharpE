namespace SharpE.Json.Schemas
{
  interface ISchemaProvider
  {
    Schema GetSchema(string path);
  }
}
