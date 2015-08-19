using System.Collections;
using System.Collections.Generic;

namespace SharpE.Json.Data
{
  public class JsonArray : JsonObject, IList<object>
  {
    private readonly List<object> m_list = new List<object>();

    public JsonArray()
      : base(-1, -1)
    {
    }

    public JsonArray(int lineIndex, int charIndex)
      : base(lineIndex, charIndex)
    {
    }

    public IEnumerator<object> GetEnumerator()
    {
      return m_list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add(object item)
    {
      m_list.Add(item);
    }

    public void Clear()
    {
      m_list.Clear();
    }

    public bool Contains(object item)
    {
      return m_list.Contains(item);
    }

    public void CopyTo(object[] array, int arrayIndex)
    {
      m_list.CopyTo(array, arrayIndex);
    }

    public bool Remove(object item)
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

    public int IndexOf(object item)
    {
      return m_list.IndexOf(item);
    }

    public void Insert(int index, object item)
    {
      m_list.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
      m_list.RemoveAt(index);
    }

    public object this[int index]
    {
      get { return m_list[index]; }
      set
      {
        m_list[index] = value;
      }
    }
  }
}
