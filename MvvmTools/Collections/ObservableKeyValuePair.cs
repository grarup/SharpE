using System.ComponentModel;
using System.Runtime.CompilerServices;
using SharpE.MvvmTools.Properties;

namespace SharpE.MvvmTools.Collections
{
  public class ObservableKeyValuePair<T, T2> : INotifyPropertyChanged
  {
    private readonly T m_key;
    private T2 m_value;

    public ObservableKeyValuePair(T key, T2 value = default(T2))
    {
      m_key = key;
      Value = value;
    }

    public T Key
    {
      get { return m_key; }
    }

    public T2 Value
    {
      get { return m_value; }
      set
      {
        if (Equals(value, m_value)) return;
        m_value = value;
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}