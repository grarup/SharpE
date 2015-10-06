using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpE.Json.Data
{
  public class JsonObject : IDocPosition
  {
    private readonly int m_lineIndex;
    private readonly int m_charIndex;

    public JsonObject(int lineIndex, int charIndex)
    {
      m_lineIndex = lineIndex;
      m_charIndex = charIndex;
    }

    public static JsonObject Load(StreamReader streamReader)
    {
      JsonException jsonException;
      JsonObject jsonObject = JsonHelperFunctions.Parse(streamReader.ReadToEnd(), out jsonException);
      if (jsonException != null)
        throw jsonException;
      return jsonObject;
    }

    public static JsonObject Load(byte[] data)
    {
      StreamReader streamReader = new StreamReader(new MemoryStream(data));
      return Load(streamReader);
    }

    public static JsonObject Load(string path)
    {
      StreamReader streamReader = File.OpenText(path);
      return Load(streamReader);
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      ToString(this, stringBuilder);
      return stringBuilder.ToString();
    }

    private void ToString(object value, StringBuilder stringBuilder, int depth = 0)
    {
      JsonNode jsonNode = value as JsonNode;
      if (jsonNode != null)
      {
        stringBuilder.Append("{\r\n");
        depth++;
        int count = 0;
        foreach (JsonElement keyValuePair in jsonNode)
        {
          Indent(stringBuilder, depth);
          stringBuilder.Append("\"" + keyValuePair.Key + "\" : ");
          ToString(keyValuePair.Value, stringBuilder, depth);
          count++;
          stringBuilder.Append(count < jsonNode.Count ? ",\r\n" : "\r\n");
        }
        depth--;
        Indent(stringBuilder, depth);
        stringBuilder.Append("}");
        return;
      }
      JsonArray jsonArray = value as JsonArray;
      if (jsonArray != null)
      {
        stringBuilder.Append("[\r\n");
        depth++;
        int listCount = 0;
        foreach (object element in jsonArray)
        {
          Indent(stringBuilder, depth);
          ToString(element, stringBuilder, depth);
          listCount++;
          stringBuilder.Append(listCount < jsonArray.Count ? ",\r\n" : "\r\n");
        }
        depth--;
        Indent(stringBuilder, depth);
        stringBuilder.Append("]");
        return;
      }
      JsonElement jsonElement = value as JsonElement;
      if (jsonElement != null)
      {

        Indent(stringBuilder, depth);
        stringBuilder.Append("{ \"" + jsonElement.Key + "\" : ");
        ToString(jsonElement.Value, stringBuilder, depth);
        stringBuilder.Append("}");
        return;
      }

      if (value is JsonValue)
      {
        value = ((JsonValue) value).Value;
      }

      if (value is string)
      {
        stringBuilder.Append("\"" + ((string)value).Replace("\\", "\\\\") + "\"");
        return;
      }

      if (value is JsonObject)
        throw new ArgumentException("Object should have been handled.");

      if (value == null)
      {
        stringBuilder.Append("null");
        return;
      }

      if (value is DateTime)
      {
        stringBuilder.Append("\"" + value + "\"");
        return;
      }

      if (value.GetType().IsEnum)
      {
        stringBuilder.Append("\"" + value + "\"");
        return;
      }

      stringBuilder.Append(value.ToString().ToLower());
    }

    private static void Indent(StringBuilder stringBuilder, int depth)
    {
      for (int i = 0; i < depth; i++)
      {
        stringBuilder.Append("  ");
      }
    }

    public object GetJsonObject(IEnumerable<string> path)
    {
      object retVal = this;
      foreach (string s in path)
      {
        JsonNode jsonNode = retVal as JsonNode;
        if (jsonNode == null || !jsonNode.ContainsKey(s))
        {
          retVal = null;
          break;
        }
        retVal = jsonNode[s];
      }
      return retVal;
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
