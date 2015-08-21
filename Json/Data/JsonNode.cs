using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpE.Json.Data
{
  public class JsonNode : JsonObject, IEnumerable<JsonElement> 
  {
    private readonly List<JsonElement> m_list = new List<JsonElement>();

    public JsonNode()
      : base(-1, -1)
    {
    }

    public JsonNode(int lineIndex, int charIndex)
      : base(lineIndex, charIndex)
    {
    }

    public IEnumerator<JsonElement> GetEnumerator()
    {
      return m_list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add(JsonElement item)
    {
      m_list.Add(item);
    }

    public void Clear()
    {
      m_list.Clear();
    }

    public bool Contains(JsonElement item)
    {
      return m_list.Contains(item);
    }

    public void CopyTo(JsonElement[] array, int arrayIndex)
    {
      m_list.CopyTo(array, arrayIndex);
    }

    public bool Remove(JsonElement item)
    {
      return m_list.Remove(item);
    }

    public int Count
    {
      get { return m_list.Count; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public bool ContainsKey(string key)
    {
      return m_list.Any(n => n.Key == key);
    }

    public void Add(string key, object value)
    {
      JsonElement jsonElement = new JsonElement(key, value);
      Add(jsonElement);
    }
    
    public void Add(string key, object value, int lineIndex, int charIndex)
    {
      JsonElement jsonElement = new JsonElement(key, value, lineIndex, charIndex);
      Add(jsonElement);
    }

    public bool Remove(string key)
    {
      JsonElement item = m_list.FirstOrDefault(n => n.Key == key);
      return item != null && Remove(item);
    }

    public bool TryGetValue(string key, out object value)
    {
      JsonElement item = m_list.FirstOrDefault(n => n.Key == key);
      if (item != null)
      {
        value = item.Value;
        return true;
      }
      value = null;
      return false;
    }

    public JsonElement this[int index]
    {
      get { return m_list[index]; }
    }

    public object this[string key]
    {
      get
      {
        JsonElement item = m_list.FirstOrDefault(n => n.Key == key);
        return item == null ? null : item.Value;
      }
      set
      {
        JsonElement item = m_list.FirstOrDefault(n => n.Key == key);
        if (item != null)
        {
          item.Value = value;
        }
        else
          Add(key, value, 0, 0);
      }
    }

    public T GetObjectOrDefault<T>(string key, T def)
    {
      JsonElement item = m_list.FirstOrDefault(n => n.Key != null && n.Key.ToLower() == key.ToLower());
      if (item == null)
        return def;
      JsonValue jsonValue = item.Value as JsonValue;
      if (typeof (T).IsEnum)
      {
        try
        {
          return (T) Enum.Parse(typeof (T), (string) (jsonValue == null ? item.Value : jsonValue.Value), true);
        }
        catch (Exception)
        {
          return def;
        }
      }
      if (typeof (T) == typeof (char))
      {
        object value = jsonValue == null ? item.Value : jsonValue.Value;
        string valueString = value as string;
        if (!string.IsNullOrEmpty(valueString))
          return (T)(object)valueString[0];
        return (T)(object)default(char);
      }
      if (typeof(T) == typeof(char) || typeof(T) == typeof(char?))
      {
        object value = jsonValue == null ? item.Value : jsonValue.Value;
        string valueString = value as string;
        if (!string.IsNullOrEmpty(valueString))
          return (T)(object)valueString[0];
        return default(T);
      }
      return (T)(jsonValue == null ? item.Value : jsonValue.Value);
    }

    public ICollection<string> Keys
    {
      get { return m_list.Select(n => n.Key).ToList(); }
    }

    public ICollection<object> Values
    {
      get { return m_list.Select(n => n.Value).ToList(); }
    }
  }
}
