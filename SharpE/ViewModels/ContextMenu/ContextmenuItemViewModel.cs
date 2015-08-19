using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SharpE.Definitions.Collection;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Properties;

namespace SharpE.ViewModels.ContextMenu
{
  public class ContextmenuItemViewModel : INotifyPropertyChanged
  {
    private readonly IObservableCollection<ContextmenuItemViewModel> m_children;
    private string m_name;
    private bool m_isVisable = true;
    private readonly string m_nameKey;
    private readonly INotifyPropertyChanged m_nameSource;
    private readonly ICommand m_command;
    private readonly INotifyPropertyChanged m_commandParameterSource;
    private readonly string m_commandParameterName;
    private object m_commandParameter;
    private readonly PropertyInfo m_commandPropertyInfo;
    private readonly PropertyInfo m_namePropertyInfo;

    public ContextmenuItemViewModel(string nameKey, INotifyPropertyChanged nameSource, ICommand command, INotifyPropertyChanged commandParameterSource, string commandParameterName)
      : this("name", new ObservableCollection<ContextmenuItemViewModel>())
    {
      m_nameKey = nameKey;
      m_nameSource = nameSource;
      m_command = command;
      m_commandParameterSource = commandParameterSource;
      m_commandParameterName = commandParameterName;
      if (m_commandParameterSource != null && m_commandParameterName != null)
      {
        m_commandPropertyInfo = m_commandParameterSource.GetType().GetProperty(m_commandParameterName);
        if (m_commandPropertyInfo != null)
        {
          m_commandParameter = m_commandPropertyInfo.GetValue(m_commandParameterSource);
          m_commandParameterSource.PropertyChanged += CommandParameterSourceOnPropertyChanged;
        }
      }
      if (nameSource != null && nameKey != null)
      {
        m_namePropertyInfo = nameSource.GetType().GetProperty(nameKey);
        if (m_namePropertyInfo != null)
        {
          m_name = m_namePropertyInfo.GetValue(nameSource) as string;
          nameSource.PropertyChanged += NameSourceOnPropertyChanged;
        }
      }
    }

    private void NameSourceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == m_nameKey)
        m_name = m_namePropertyInfo.GetValue(m_nameSource) as string;
    }


    public ContextmenuItemViewModel(string name, ICommand command, INotifyPropertyChanged commandParameterSource = null, string commandParameterName= null) : this(name, new ObservableCollection<ContextmenuItemViewModel>())
    {
      m_command = command;
      m_commandParameterSource = commandParameterSource;
      m_commandParameterName = commandParameterName;
      if (m_commandParameterSource != null && m_commandParameterName != null)
      {
        m_commandPropertyInfo = m_commandParameterSource.GetType().GetProperty(m_commandParameterName);
        if (m_commandPropertyInfo != null)
        {
          m_commandParameter = m_commandPropertyInfo.GetValue(m_commandParameterSource);
          m_commandParameterSource.PropertyChanged += CommandParameterSourceOnPropertyChanged;
        }
      }
    }

    public ContextmenuItemViewModel(string nameKey, INotifyPropertyChanged nameSource, ICommand command, object commandParameter)
      : this("Name", new ObservableCollection<ContextmenuItemViewModel>())
    {
      m_nameKey = nameKey;
      m_nameSource = nameSource;
      m_command = command;
      m_commandParameter = commandParameter;
      if (nameSource != null && nameKey != null)
      {
        m_namePropertyInfo = nameSource.GetType().GetProperty(nameKey);
        if (m_namePropertyInfo != null)
        {
          m_name = m_namePropertyInfo.GetValue(nameSource) as string;
          nameSource.PropertyChanged += NameSourceOnPropertyChanged;
        }
      }
    }

    public ContextmenuItemViewModel(string name, ICommand command, object commandParameter) : this(name, new ObservableCollection<ContextmenuItemViewModel>())
    {
      m_command = command;
      m_commandParameter = commandParameter;
    }

    public ContextmenuItemViewModel(string name) : this(name, new ObservableCollection<ContextmenuItemViewModel>())
    {
    }

    public ContextmenuItemViewModel(string name, IObservableCollection<ContextmenuItemViewModel> children)
    {
      m_name = name;
      m_children = children;
    }
    
    private void CommandParameterSourceOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (propertyChangedEventArgs.PropertyName == m_commandParameterName)
        m_commandParameter = m_commandPropertyInfo.GetValue(m_commandParameterSource);
    }

    public string Name
    {
      get { return m_name; }
    }

    public IObservableCollection<ContextmenuItemViewModel> Children
    {
      get { return m_children; }
    }

    public bool IsVisable
    {
      get { return m_isVisable; }
      set
      {
        if (value.Equals(m_isVisable)) return;
        m_isVisable = value;
        OnPropertyChanged();
      }
    }

    public object CommandParameter
    {
      get { return m_commandParameter; }
      set
      {
        if (Equals(value, m_commandParameter)) return;
        m_commandParameter = value;
        OnPropertyChanged();
      }
    }

    public ICommand Command
    {
      get { return m_command; }
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
