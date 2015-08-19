using System;
using System.Linq;
using SharpE.Json.Data;

namespace SharpE.Json.Schemas
{
  public static class SchemaHelper
  {
    public static JsonNode GenerateSchema(JsonNode jsonNode, bool isRoot = true)
    {
      if (jsonNode == null)
        return null;
      JsonNode retVal = new JsonNode(); 
      JsonNode properties = new JsonNode();
      if (isRoot)
      {
        retVal.Add(new JsonElement("$schema", "quickschema.schema.json"));
        JsonNode schemaProperty = new JsonNode {new JsonElement("type", "string")};
        properties.Add("$schema", schemaProperty);
      }
      retVal.Add(new JsonElement("type", "object"));
      retVal.Add("properties", properties,-1,-1);
      foreach (JsonElement jsonElement in jsonNode)
      {
        properties.Add(new JsonElement(jsonElement.Key,GenerateProperty(jsonElement.Value)));
      }
      return retVal;
    }

    private static JsonNode GenerateProperty(object value)
    {
      if (value == null)
        return null;
      JsonNode property = new JsonNode();
      Type type = value.GetType();
      if (type == typeof (JsonValue))
        type = ((JsonValue) value).Value.GetType();
      if (type == typeof (string))
      {
        property.Add(new JsonElement("type", "string"));
      }
      if (type == typeof (bool))
      {
        property.Add(new JsonElement("type", "bool"));
      }
      if (type == typeof (int))
      {
        property.Add(new JsonElement("type", "integer"));
      }
      if (type == typeof (double))
      {
        property.Add(new JsonElement("type", "number"));
      }
      else if (type == typeof (JsonNode))
      {
        property = GenerateSchema((JsonNode) value, false);
      }
      else if (type == typeof (JsonArray))
      {
        property.Add(new JsonElement("type", "array"));
        JsonNode jsonNode = ((JsonArray) value).FirstOrDefault() as JsonNode;
        property.Add(jsonNode != null
          ? new JsonElement("items", GenerateSchema(jsonNode, false))
          : new JsonElement("items", GenerateProperty(((JsonArray) value).FirstOrDefault())));
      }
      return property;
    }


  }
}
