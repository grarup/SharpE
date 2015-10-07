using System;
using System.ComponentModel;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows;
using SharpE.MvvmTools.Collections;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Properties;

namespace SharpE.MvvmTools.Helpers
{
  public class MessageBoxViewModel : INotifyPropertyChanged
  {
    private readonly ResourceManager m_resourceManager;
    private readonly AsyncObservableCollection<MessageBoxButtonViewModel> m_buttons = new AsyncObservableCollection<MessageBoxButtonViewModel>();
    private string m_title;
    private string m_text;
    private object[] m_values;
    private MessageBoxResult m_result;
    private bool m_isShown;
    private MessageBoxButton? m_button;
    private readonly ManualCommand m_closeCommand;
    private Action<MessageBoxResult, object[]> m_resultFunction;
    private UIElement m_icon;

    public MessageBoxViewModel(ResourceManager resourceManager = null)
    {
      m_resourceManager = resourceManager;
      m_closeCommand = new ManualCommand(Close);
    }

    private void Close()
    {
      Result = MessageBoxResult.Cancel;
      IsShown = false;
    }

    public MessageBoxButton? Button
    {
      get { return m_button; }
      set
      {
        if (m_button == value) return;
        m_button = value;
        Buttons.Clear();
        switch (m_button)
        {
          case MessageBoxButton.OK:
            Buttons.Add(new MessageBoxButtonViewModel(GetString(MessageBoxResult.OK), MessageBoxResult.OK, ButtonPushed, true));
            break;
          case MessageBoxButton.OKCancel:
            Buttons.Add(new MessageBoxButtonViewModel(GetString(MessageBoxResult.OK), MessageBoxResult.OK, ButtonPushed));
            Buttons.Add(new MessageBoxButtonViewModel(GetString(MessageBoxResult.Cancel), MessageBoxResult.Cancel, ButtonPushed, true));
            break;
          case MessageBoxButton.YesNoCancel:
            Buttons.Add(new MessageBoxButtonViewModel(GetString(MessageBoxResult.Yes), MessageBoxResult.Yes, ButtonPushed));
            Buttons.Add(new MessageBoxButtonViewModel(GetString(MessageBoxResult.No), MessageBoxResult.No, ButtonPushed));
            Buttons.Add(new MessageBoxButtonViewModel(GetString(MessageBoxResult.Cancel), MessageBoxResult.Cancel, ButtonPushed, true));
            break;
          case MessageBoxButton.YesNo:
            Buttons.Add(new MessageBoxButtonViewModel(GetString(MessageBoxResult.Yes), MessageBoxResult.Yes, ButtonPushed, true));
            Buttons.Add(new MessageBoxButtonViewModel(GetString(MessageBoxResult.No), MessageBoxResult.No, ButtonPushed));
            break;
        }
        OnPropertyChanged();
      }
    }

    private string GetString(MessageBoxResult result)
    {
      string retval = null;
      if (m_resourceManager != null)
        retval = m_resourceManager.GetString("MessageBoxResult_" + Enum.GetName(typeof(MessageBoxResult), result));
      return retval ?? Enum.GetName(typeof (MessageBoxResult), result);
    }

    public bool IsShown
    {
      get { return m_isShown; }
      set
      {
        if (value.Equals(m_isShown)) return;
        m_isShown = value;
        if (value)
          m_result = MessageBoxResult.None;
        OnPropertyChanged();
      }
    }

    public MessageBoxResult Result
    {
      get { return m_result; }
      private set
      {
        if (value == m_result) return;
        m_result = value;
        if (m_resultFunction != null)
          m_resultFunction.Invoke(m_result, Values);
        OnPropertyChanged();
      }
    }

    public string Text
    {
      get { return m_text; }
      set
      {
        if (value == m_text) return;
        m_text = value;
        OnPropertyChanged();
      }
    }

    public string Title
    {
      get { return m_title; }
      set
      {
        if (value == m_title) return;
        m_title = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand CloseCommand
    {
      get { return m_closeCommand; }
    }

    public AsyncObservableCollection<MessageBoxButtonViewModel> Buttons
    {
      get { return m_buttons; }
    }

    public Action<MessageBoxResult, object[]> ResultFunction
    {
      get { return m_resultFunction; }
      set { m_resultFunction = value; }
    }

    public UIElement Icon
    {
      get { return m_icon; }
      set
      {
        if (Equals(value, m_icon)) return;
        m_icon = value;
        OnPropertyChanged();
      }
    }

    public object[] Values
    {
      get { return m_values; }
      set { m_values = value; }
    }

    private void ButtonPushed(MessageBoxResult result)
    {
      IsShown = false;
      Result = result;
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
