using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SharpE.MvvmTools.Helpers
{
  public enum ButtonDirection
  {
    Down,
    Up,
  }

  public class MouseAction : FrameworkElement
  {
    #region declerations
    private bool m_isFirstButtonDwon;
    private bool m_isSecondButtonDown;
    private bool m_isActive;
    private object m_dataContext;
    private ICommand m_action;
    #endregion

    #region dependecy properties

    public static readonly DependencyProperty ButtonDirectionProperty =
      DependencyProperty.Register("ButtonDirection", typeof(ButtonDirection), typeof(MouseAction),
                                  new UIPropertyMetadata());

    public static readonly DependencyProperty MouseButtonProperty =
      DependencyProperty.Register("MouseButton", typeof(MouseButton), typeof(MouseAction),
                                  new UIPropertyMetadata());

    public static readonly DependencyProperty ActionProperty =
      DependencyProperty.Register("Action", typeof(string), typeof(MouseAction), new PropertyMetadata(default(string), ActionPropertyChangedCallback));

    private static void ActionPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      MouseAction mouseAction = dependencyObject as MouseAction;
      if (mouseAction == null) return;
      mouseAction.m_dataContext = null;
      mouseAction.m_action = null;
    }

    public static readonly DependencyProperty SecondMouseButtonProperty =
      DependencyProperty.Register("SecondMouseButton", typeof(MouseButton?), typeof(MouseAction),
                                  new PropertyMetadata(default(MouseButton?)));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
      "CommandParameter", typeof(object), typeof(MouseAction), new PropertyMetadata(default(object)));

    #endregion

    #region public properites

    public MouseButton? SecondMouseButton
    {
      get { return (MouseButton?)GetValue(SecondMouseButtonProperty); }
      set { SetValue(SecondMouseButtonProperty, value); }
    }

    public ButtonDirection ButtonDirection
    {
      get { return (ButtonDirection)GetValue(ButtonDirectionProperty); }
      set { SetValue(ButtonDirectionProperty, value); }
    }

    public MouseButton MouseButton
    {
      get { return (MouseButton)GetValue(MouseButtonProperty); }
      set { SetValue(MouseButtonProperty, value); }
    }

    public string Action
    {
      get { return (string)GetValue(ActionProperty); }
      set { SetValue(ActionProperty, value); }
    }

    public object CommandParameter
    {
      get { return GetValue(CommandParameterProperty); }
      set { SetValue(CommandParameterProperty, value); }
    }

    public bool Handle(MouseButtonEventArgs args, ButtonDirection buttonDirection, object dataContex)
    {
      if (m_dataContext != dataContex)
      {
        PropertyInfo property = dataContex.GetType().GetProperty(Action);
        if (property == null) return false;
        m_action = property.GetValue(dataContex) as ICommand;
        m_dataContext = dataContex;
      }
      if (m_action == null) return false;
      if (SecondMouseButton != null)
      {
        if (args.ChangedButton == MouseButton)
          m_isFirstButtonDwon = buttonDirection == ButtonDirection.Down;
        if (args.ChangedButton == SecondMouseButton)
          m_isSecondButtonDown = buttonDirection == ButtonDirection.Down;
        if (m_isFirstButtonDwon && m_isSecondButtonDown)
        {
          m_isActive = true;
          if (ButtonDirection == ButtonDirection.Down)
            m_action.Execute(CommandParameter);
          return true;
        }
        if ((!m_isFirstButtonDwon || !m_isSecondButtonDown) && m_isActive)
        {
          if (ButtonDirection == ButtonDirection.Up)
            m_action.Execute(CommandParameter);
          m_isActive = false;
          return true;
        }
        return false;
      }

      if (args.ChangedButton != MouseButton) return false;
      if (buttonDirection != ButtonDirection) return false;
      if (m_action == null) return false;
      object parameter = CommandParameter ?? dataContex;
      m_action.Execute(parameter);
      return true;
    }
    #endregion
  }
}
