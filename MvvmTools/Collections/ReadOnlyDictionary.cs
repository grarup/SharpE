using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpE.MvvmTools.Collections
{
  public class ReadOnlyDictionary<T,T2> : IDictionary<T, T2>
  {
    private readonly IDictionary<T, T2> m_source;

    public ReadOnlyDictionary(IDictionary<T, T2> source)
    {
      m_source = source;
    }

    public IEnumerator<KeyValuePair<T, T2>> GetEnumerator()
    {
      return m_source.Select(keyValuePair => new KeyValuePair<T, T2>(keyValuePair.Key, keyValuePair.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add(KeyValuePair<T, T2> item)
    {
    }

    public void Clear()
    {
    }

    public bool Contains(KeyValuePair<T, T2> item)
    {
      return m_source.Contains(item);
    }

    public void CopyTo(KeyValuePair<T, T2>[] array, int arrayIndex)
    {
      m_source.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<T, T2> item)
    {
      return false;
    }

    public int Count
    {
      get { return m_source.Count; }
    }

    public bool IsReadOnly
    {
      get { return true; }
    }

    public bool ContainsKey(T key)
    {
      return m_source.ContainsKey(key);
    }

    public void Add(T key, T2 value)
    {
      
    }

    public bool Remove(T key)
    {
      return false;
    }

    public bool TryGetValue(T key, out T2 value)
    {
      return m_source.TryGetValue(key, out value);
    }

    public T2 this[T key]
    {
      get { return m_source[key]; }
      set {  }
    }

    public ICollection<T> Keys
    {
      get { return m_source.Keys; }
    }

    public ICollection<T2> Values
    {
      get { return m_source.Values; }
    }
  }
}
