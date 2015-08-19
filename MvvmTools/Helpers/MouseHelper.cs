using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SharpE.MvvmTools.Helpers
{
  public class MouseHelper
  {
    #region MouseActions
    internal static readonly DependencyProperty MouseActionsProperty =
      DependencyProperty.RegisterAttached("InternalMouseActions", typeof(MouseActionCollection), typeof(MouseHelper), new PropertyMetadata(default(MouseActionCollection), MouseActionPropertyChangedCallback));

    private static void MouseActionPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement element = dependencyObject as UIElement;
      if (element == null) return;

      MouseActionCollection oldValue = dependencyPropertyChangedEventArgs.OldValue as MouseActionCollection;
      if (oldValue != null)
      {
        element.PreviewMouseDown -= UiElementOnMouseDownAction;
        element.PreviewMouseUp -= UiElementOnMouseUpAction;
      }

      MouseActionCollection newValue = dependencyPropertyChangedEventArgs.NewValue as MouseActionCollection;
      if (newValue != null)
      {
        element.PreviewMouseDown += UiElementOnMouseDownAction;
        element.PreviewMouseUp += UiElementOnMouseUpAction;
      }

    }

    private static void UiElementOnMouseUpAction(object sender, MouseButtonEventArgs e)
    {
      UIElement uiElement = (UIElement)sender;
      FrameworkElement frameworkElement = sender as FrameworkElement;
      foreach (MouseAction mouseAction in GetMouseActions(uiElement).Actions)
      {
        e.Handled |= mouseAction.Handle(e, ButtonDirection.Up, frameworkElement == null ? null : frameworkElement.DataContext);
      }
    }

    private static void UiElementOnMouseDownAction(object sender, MouseButtonEventArgs e)
    {
      UIElement uiElement = (UIElement)sender;
      FrameworkElement frameworkElement = sender as FrameworkElement;
      foreach (MouseAction mouseAction in GetMouseActions(uiElement).Actions)
      {
        e.Handled |= mouseAction.Handle(e, ButtonDirection.Down, frameworkElement == null ? null : frameworkElement.DataContext);
      }
    }

    public static void SetMouseActions(UIElement element, MouseActionCollection value)
    {
      element.SetValue(MouseActionsProperty, value);
    }

    public static MouseActionCollection GetMouseActions(UIElement element)
    {
      return (MouseActionCollection)element.GetValue(MouseActionsProperty);
    }

    #endregion

    #region mouseDown

    public static readonly DependencyProperty MouseDownProperty =
      DependencyProperty.RegisterAttached("MouseDown", typeof(ICommand), typeof(MouseHelper), new FrameworkPropertyMetadata(default(ICommand), MouseDownPropertyChangedCallback));

    private static void MouseDownPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = (UIElement)dependencyObject;

      ICommand oldElement = dependencyPropertyChangedEventArgs.OldValue as ICommand;
      if (oldElement != null)
        uiElement.PreviewMouseDown -= UiElementOnMouseDown;

      ICommand newElement = dependencyPropertyChangedEventArgs.NewValue as ICommand;
      if (newElement != null)
        uiElement.PreviewMouseDown += UiElementOnMouseDown;
    }

    public static void SetMouseDown(UIElement element, ICommand value)
    {
      element.SetValue(MouseDownProperty, value);
    }

    public static ICommand GetMouseDown(UIElement element)
    {
      return (ICommand)element.GetValue(MouseDownProperty);
    }

    private static void UiElementOnMouseDown(object sender, MouseButtonEventArgs mouseEventArgs)
    {
      UIElement uiElement = (UIElement)sender;
      if (GetMouseDownButton(uiElement) != mouseEventArgs.ChangedButton) return;
      FrameworkElement frameworkElement = sender as FrameworkElement;
      GetMouseDown(uiElement).Execute(frameworkElement == null ? null : frameworkElement.DataContext);
    }

    public static readonly DependencyProperty MouseDownButtonProperty =
      DependencyProperty.RegisterAttached("MouseDownButton", typeof(MouseButton), typeof(MouseHelper), new PropertyMetadata(default(MouseButton)));

    public static void SetMouseDownButton(UIElement element, MouseButton value)
    {
      element.SetValue(MouseDownButtonProperty, value);
    }

    public static MouseButton GetMouseDownButton(UIElement element)
    {
      return (MouseButton)element.GetValue(MouseDownButtonProperty);
    }
    #endregion

    #region mouse up
    public static readonly DependencyProperty MouseUpProperty =
      DependencyProperty.RegisterAttached("MouseUp", typeof(ICommand), typeof(MouseHelper), new FrameworkPropertyMetadata(default(ICommand), MouseUpPropertyChangedCallback));

    private static void MouseUpPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = (UIElement)dependencyObject;

      ICommand oldElement = dependencyPropertyChangedEventArgs.OldValue as ICommand;
      if (oldElement != null)
        uiElement.MouseUp -= UiElementOnMouseUp;

      ICommand newElement = dependencyPropertyChangedEventArgs.NewValue as ICommand;
      if (newElement != null)
        uiElement.MouseUp += UiElementOnMouseUp;

    }

    public static void SetMouseUp(UIElement element, ICommand value)
    {
      element.SetValue(MouseUpProperty, value);
    }

    public static ICommand GetMouseUp(UIElement element)
    {
      return (ICommand)element.GetValue(MouseUpProperty);
    }

    public static readonly DependencyProperty MouseUpButtonProperty =
      DependencyProperty.RegisterAttached("MouseUpButton", typeof(MouseButton), typeof(MouseHelper), new PropertyMetadata(default(MouseButton)));

    public static void SetMouseUpButton(UIElement element, MouseButton value)
    {
      element.SetValue(MouseUpButtonProperty, value);
    }

    public static MouseButton GetMouseUpButton(UIElement element)
    {
      return (MouseButton)element.GetValue(MouseUpButtonProperty);
    }

    private static void UiElementOnMouseUp(object sender, MouseButtonEventArgs mouseEventArgs)
    {
      UIElement uiElement = (UIElement)sender;
      if (GetMouseUpButton(uiElement) != mouseEventArgs.ChangedButton) return;
      FrameworkElement frameworkElement = sender as FrameworkElement;
      GetMouseUp(uiElement).Execute(frameworkElement == null ? null : frameworkElement.DataContext);
      mouseEventArgs.Handled = true;

    }
    #endregion

    #region is mouse down
    public static readonly DependencyProperty MouseIsDownProperty =
      DependencyProperty.RegisterAttached("MouseIsDown", typeof(Action<bool>), typeof(MouseHelper), new FrameworkPropertyMetadata(default(PropertyPath), MouseIsDownPropertyChangedCallback));

    private static void MouseIsDownPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = (UIElement)dependencyObject;

      Action<bool> oldElement = dependencyPropertyChangedEventArgs.OldValue as Action<bool>;
      if (oldElement != null)
      {
        uiElement.PreviewMouseDown -= MouseDownForIsMouseDown;
        uiElement.PreviewMouseUp -= MouseUpForIsMouseDown;
      }

      Action<bool> newElement = dependencyPropertyChangedEventArgs.NewValue as Action<bool>;
      if (newElement != null)
      {
        uiElement.PreviewMouseDown += MouseDownForIsMouseDown;
        uiElement.PreviewMouseUp += MouseUpForIsMouseDown;
      }
    }

    private static void MouseUpForIsMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      UIElement uiElement = (UIElement)sender;
      if (GetIgnoreIsMouseDown(uiElement)) return;
      if (GetMouseDownButton(uiElement) != mouseButtonEventArgs.ChangedButton) return;
      if (GetIsMouseDownDelay(uiElement) != 0)
      {
        DispatcherTimer dispatcherTimer = GetIsMouseDownDispatcher(uiElement);
        if (dispatcherTimer != null)
          dispatcherTimer.Stop();
      }
      GetMouseIsDown(uiElement).Invoke(false);
    }

    private static void DispatcherTimerOnTick(object sender, EventArgs eventArgs)
    {
      DispatcherTimer dispatcherTimer = (DispatcherTimer)sender;
      dispatcherTimer.Stop();
      GetMouseIsDown((UIElement)dispatcherTimer.Tag).Invoke(true);
    }

    private static void MouseDownForIsMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      UIElement uiElement = (UIElement)sender;
      if (GetIgnoreIsMouseDown(uiElement)) return;
      if (GetMouseDownButton(uiElement) != mouseButtonEventArgs.ChangedButton) return;
      if (GetIsMouseDownDelay(uiElement) != 0)
      {
        DispatcherTimer dispatcherTimer = GetIsMouseDownDispatcher(uiElement);
        if (dispatcherTimer == null)
        {
          dispatcherTimer = new DispatcherTimer();
          dispatcherTimer.Tick += DispatcherTimerOnTick;
          dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, GetIsMouseDownDelay(uiElement));
          dispatcherTimer.Tag = uiElement;
          SetIsMouseDownDispatcher(uiElement, dispatcherTimer);
        }
        dispatcherTimer.Start();

      }
      else
        GetMouseIsDown(uiElement).Invoke(true);
    }

    public static void SetMouseIsDown(UIElement element, Action<bool> value)
    {
      element.SetValue(MouseIsDownProperty, value);
    }

    public static Action<bool> GetMouseIsDown(UIElement element)
    {
      return (Action<bool>)element.GetValue(MouseIsDownProperty);
    }

    public static readonly DependencyProperty IsMouseDownDelayProperty =
      DependencyProperty.RegisterAttached("IsMouseDownDelay", typeof(int), typeof(MouseHelper), new PropertyMetadata(default(int)));

    public static void SetIsMouseDownDelay(UIElement element, int value)
    {
      element.SetValue(IsMouseDownDelayProperty, value);
    }

    public static int GetIsMouseDownDelay(UIElement element)
    {
      return (int)element.GetValue(IsMouseDownDelayProperty);
    }

    public static readonly DependencyProperty IgnoreIsMouseDownProperty =
      DependencyProperty.RegisterAttached("IgnoreIsMouseDown", typeof(bool), typeof(MouseHelper), new PropertyMetadata(default(bool)));

    public static void SetIgnoreIsMouseDown(UIElement element, bool value)
    {
      element.SetValue(IgnoreIsMouseDownProperty, value);
    }

    public static bool GetIgnoreIsMouseDown(UIElement element)
    {
      return (bool)element.GetValue(IgnoreIsMouseDownProperty);
    }

    private static readonly DependencyProperty s_isMouseDownDispatcherProperty =
      DependencyProperty.RegisterAttached("IsMouseDownDispatcher", typeof(DispatcherTimer), typeof(MouseHelper), new PropertyMetadata(default(DispatcherTimer)));

    private static void SetIsMouseDownDispatcher(UIElement element, DispatcherTimer value)
    {
      element.SetValue(s_isMouseDownDispatcherProperty, value);
    }

    private static DispatcherTimer GetIsMouseDownDispatcher(UIElement element)
    {
      return (DispatcherTimer)element.GetValue(s_isMouseDownDispatcherProperty);
    }
    #endregion

    #region is mouse over
    public static readonly DependencyProperty MouseIsOverProperty =
      DependencyProperty.RegisterAttached("MouseIsOver", typeof(Action<bool>), typeof(MouseHelper), new FrameworkPropertyMetadata(default(Action<bool>), MouseIsOverPropertyChangedCallback));

    private static void MouseIsOverPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = (UIElement)dependencyObject;

      Action<bool> oldElement = dependencyPropertyChangedEventArgs.OldValue as Action<bool>;
      if (oldElement != null)
      {
        uiElement.MouseEnter -= OnMouseEnterForMouseIsOver;
        uiElement.MouseLeave -= OnMouseLeaveForMouseIsOver;
      }

      Action<bool> newElement = dependencyPropertyChangedEventArgs.NewValue as Action<bool>;
      if (newElement != null)
      {
        uiElement.MouseEnter += OnMouseEnterForMouseIsOver;
        uiElement.MouseLeave += OnMouseLeaveForMouseIsOver;
      }
    }

    private static void OnMouseLeaveForMouseIsOver(object sender, MouseEventArgs mouseEventArgs)
    {
      UIElement uiElement = (UIElement)sender;
      if (GetIgnoreMouseIsOver(uiElement)) return;

      if (GetIsMouseOverDelay(uiElement) != 0)
      {
        DispatcherTimer dispatcherTimer = GetIsMouseOverDispatcherTimer(uiElement);
        if (dispatcherTimer != null)
          dispatcherTimer.Stop();
      }

      GetMouseIsOver(uiElement).Invoke(false);
    }

    private static void OnMouseEnterForMouseIsOver(object sender, MouseEventArgs mouseEventArgs)
    {
      UIElement uiElement = (UIElement)sender;
      if (GetIgnoreMouseIsOver(uiElement)) return;

      if (GetIsMouseOverDelay(uiElement) != 0)
      {
        DispatcherTimer dispatcherTimer = GetIsMouseOverDispatcherTimer(uiElement);
        if (dispatcherTimer == null)
        {
          dispatcherTimer = new DispatcherTimer();
          dispatcherTimer.Tick += IsMouseOverDispatcherTimerOnTick;
          dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, GetIsMouseDownDelay(uiElement));
          dispatcherTimer.Tag = uiElement;
          SetIsMouseOverDispatcherTimer(uiElement, dispatcherTimer);
        }
        dispatcherTimer.Start();

      }
      else
        GetMouseIsOver(uiElement).Invoke(true);
    }

    private static void IsMouseOverDispatcherTimerOnTick(object sender, EventArgs e)
    {
      DispatcherTimer dispatcherTimer = (DispatcherTimer)sender;
      dispatcherTimer.Stop();
      GetMouseIsOver((UIElement)dispatcherTimer.Tag).Invoke(true);
    }

    public static void SetMouseIsOver(UIElement element, Action<bool> value)
    {
      element.SetValue(MouseIsOverProperty, value);
    }

    public static Action<bool> GetMouseIsOver(UIElement element)
    {
      return (Action<bool>)element.GetValue(MouseIsOverProperty);
    }

    public static readonly DependencyProperty IgnoreMouseIsOverProperty =
      DependencyProperty.RegisterAttached("IgnoreMouseIsOver", typeof(bool), typeof(MouseHelper), new PropertyMetadata(default(bool)));

    public static void SetIgnoreMouseIsOver(UIElement element, bool value)
    {
      element.SetValue(IgnoreMouseIsOverProperty, value);
    }

    public static bool GetIgnoreMouseIsOver(UIElement element)
    {
      return (bool)element.GetValue(IgnoreMouseIsOverProperty);
    }

    public static readonly DependencyProperty IsMouseOverDelayProperty =
      DependencyProperty.RegisterAttached("IsMouseOverDelay", typeof(int), typeof(MouseHelper), new PropertyMetadata(default(int)));

    public static void SetIsMouseOverDelay(UIElement element, int value)
    {
      element.SetValue(IsMouseOverDelayProperty, value);
    }

    public static int GetIsMouseOverDelay(UIElement element)
    {
      return (int)element.GetValue(IsMouseOverDelayProperty);
    }

    private static readonly DependencyProperty IsMouseOverDispatcherTimerProperty =
      DependencyProperty.RegisterAttached("IsMouseOverDispatcherTimer", typeof(DispatcherTimer), typeof(MouseHelper), new PropertyMetadata(default(DispatcherTimer)));

    private static void SetIsMouseOverDispatcherTimer(UIElement element, DispatcherTimer value)
    {
      element.SetValue(IsMouseOverDispatcherTimerProperty, value);
    }

    private static DispatcherTimer GetIsMouseOverDispatcherTimer(UIElement element)
    {
      return (DispatcherTimer)element.GetValue(IsMouseOverDispatcherTimerProperty);
    }
    #endregion

    #region mouse doubleclick

    public static readonly DependencyProperty MouseDoubleClickProperty =
      DependencyProperty.RegisterAttached("MouseDoubleClick", typeof(ICommand), typeof(MouseHelper), new PropertyMetadata(default(ICommand), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null) return;

      ICommand oldElement = dependencyPropertyChangedEventArgs.OldValue as ICommand;
      if (oldElement != null)
        uiElement.MouseDown -= DoublcClkDetectOnMouseDown;

      ICommand newElement = dependencyPropertyChangedEventArgs.NewValue as ICommand;
      if (newElement != null)
        uiElement.MouseDown += DoublcClkDetectOnMouseDown;
    }

    private static void DoublcClkDetectOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      if (mouseButtonEventArgs.ClickCount == 2 && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
      {
        FrameworkElement frameworkElement = sender as FrameworkElement;
        ICommand command = GetMouseDoubleClick((UIElement)sender);
        command.Execute(frameworkElement == null ? null : frameworkElement.DataContext);
        mouseButtonEventArgs.Handled = true;
      }
    }

    public static void SetMouseDoubleClick(UIElement element, ICommand value)
    {
      element.SetValue(MouseDoubleClickProperty, value);
    }

    public static ICommand GetMouseDoubleClick(UIElement element)
    {
      return (ICommand)element.GetValue(MouseDoubleClickProperty);
    }
    #endregion

    #region mouse scroll

    public static readonly DependencyProperty MouseScrollProperty =
      DependencyProperty.RegisterAttached("MouseScroll", typeof(Action<int>), typeof(MouseHelper), new PropertyMetadata(default(Action<int>), MouseScrollPropertyChangedCallback));

    private static void MouseScrollPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null) return;

      Action<int> oldAction = dependencyPropertyChangedEventArgs.OldValue as Action<int>;
      if (oldAction != null)
        uiElement.MouseWheel -= UiElementOnMouseWheel;

      Action<int> newAction = dependencyPropertyChangedEventArgs.NewValue as Action<int>;
      if (newAction != null)
        uiElement.MouseWheel += UiElementOnMouseWheel;

    }

    private static void UiElementOnMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
    {
      Action<int> action = GetMouseScroll((UIElement)sender);
      action.Invoke(mouseWheelEventArgs.Delta);
    }

    public static void SetMouseScroll(UIElement element, Action<int> value)
    {
      element.SetValue(MouseScrollProperty, value);
    }

    public static Action<int> GetMouseScroll(UIElement element)
    {
      return (Action<int>)element.GetValue(MouseScrollProperty);
    }

    public static readonly DependencyProperty MouseScrollUpProperty =
      DependencyProperty.RegisterAttached("MouseScrollUp", typeof(ICommand), typeof(MouseHelper), new PropertyMetadata(default(ICommand), MouseScrollUpPropertyChangedCallback));

    private static void MouseScrollUpPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null) return;

      ICommand oldAction = dependencyPropertyChangedEventArgs.OldValue as ICommand;
      if (oldAction != null)
        uiElement.MouseWheel -= UiElementOnMouseWheelForUp;

      ICommand newAction = dependencyPropertyChangedEventArgs.NewValue as ICommand;
      if (newAction != null)
        uiElement.MouseWheel += UiElementOnMouseWheelForUp;
    }

    private static void UiElementOnMouseWheelForUp(object sender, MouseWheelEventArgs e)
    {
      if (e.Delta <= 0) return;
      for (int i = 0; i < e.Delta; i += 120)
      {
        GetMouseScrollUp((UIElement)sender).Execute(null);
      }
      e.Handled = true;
    }

    public static void SetMouseScrollUp(UIElement element, ICommand value)
    {
      element.SetValue(MouseScrollUpProperty, value);
    }

    public static ICommand GetMouseScrollUp(UIElement element)
    {
      return (ICommand)element.GetValue(MouseScrollUpProperty);
    }

    public static readonly DependencyProperty MouseScrollDownProperty =
      DependencyProperty.RegisterAttached("MouseScrollDown", typeof(ICommand), typeof(MouseHelper), new PropertyMetadata(default(ICommand), MouseScrollDownPropertyChangedCallback));

    private static void MouseScrollDownPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null) return;

      ICommand oldAction = dependencyPropertyChangedEventArgs.OldValue as ICommand;
      if (oldAction != null)
        uiElement.MouseWheel -= UiElementOnMouseWheelForDown;

      ICommand newAction = dependencyPropertyChangedEventArgs.NewValue as ICommand;
      if (newAction != null)
        uiElement.MouseWheel += UiElementOnMouseWheelForDown;
    }

    private static void UiElementOnMouseWheelForDown(object sender, MouseWheelEventArgs e)
    {
      if (e.Delta >= 0) return;
      for (int i = 0; i > e.Delta; i -= 120)
      {
        GetMouseScrollDown((UIElement)sender).Execute(null);
      }
      e.Handled = true;
    }

    public static void SetMouseScrollDown(UIElement element, ICommand value)
    {
      element.SetValue(MouseScrollDownProperty, value);
    }

    public static ICommand GetMouseScrollDown(UIElement element)
    {
      return (ICommand)element.GetValue(MouseScrollDownProperty);
    }
    #endregion

    #region MouseClickToggleBool
    public static readonly DependencyProperty MouseClicktoggleBoolProperty =
      DependencyProperty.RegisterAttached("MouseClicktoggleBool", typeof(bool?), typeof(MouseHelper), new FrameworkPropertyMetadata(default(bool?), MouseClickTogglePropertyChanged) { BindsTwoWayByDefault = true });

    private static void MouseClickTogglePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null) return;

      uiElement.MouseDown -= UiElement2OnMouseDown;
      uiElement.MouseDown += UiElement2OnMouseDown;
    }

    private static void UiElement2OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      if (GetMouseToggleButton((UIElement)sender) != mouseButtonEventArgs.ChangedButton) return;
      bool? value = GetMouseClicktoggleBool((UIElement)sender);
      if (value.HasValue)
      {
        SetMouseClicktoggleBool((UIElement)sender, !value.Value);
      }
    }


    public static void SetMouseClicktoggleBool(UIElement element, bool? value)
    {
      element.SetValue(MouseClicktoggleBoolProperty, value);
    }

    public static bool? GetMouseClicktoggleBool(UIElement element)
    {
      return (bool?)element.GetValue(MouseClicktoggleBoolProperty);
    }

    public static readonly DependencyProperty MouseToggleButtonProperty =
      DependencyProperty.RegisterAttached("MouseToggleButton", typeof(MouseButton), typeof(MouseHelper), new PropertyMetadata(default(MouseButton)));

    public static void SetMouseToggleButton(UIElement element, MouseButton value)
    {
      element.SetValue(MouseToggleButtonProperty, value);
    }

    public static MouseButton GetMouseToggleButton(UIElement element)
    {
      return (MouseButton)element.GetValue(MouseToggleButtonProperty);
    }
    #endregion

    #region MouseEnter

    public static readonly DependencyProperty MouseEnterProperty =
      DependencyProperty.RegisterAttached("MouseEnter", typeof(ICommand), typeof(MouseHelper), new PropertyMetadata(default(ICommand), MouseEnterPropertyChangedCallback));

    private static void MouseEnterPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null) return;

      ICommand oldValue = dependencyPropertyChangedEventArgs.OldValue as ICommand;
      if (oldValue != null)
        uiElement.MouseEnter -= UiElementOnMouseEnter;

      ICommand newValue = dependencyPropertyChangedEventArgs.NewValue as ICommand;
      if (newValue != null)
        uiElement.MouseEnter += UiElementOnMouseEnter;
    }

    private static void UiElementOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
    {
      if (GetIgnoreMouseEnter((UIElement)sender)) return;
      GetMouseEnter((UIElement)sender).Execute(null);
    }

    public static void SetMouseEnter(UIElement element, ICommand value)
    {
      element.SetValue(MouseEnterProperty, value);
    }

    public static ICommand GetMouseEnter(UIElement element)
    {
      return (ICommand)element.GetValue(MouseEnterProperty);
    }

    public static readonly DependencyProperty IgnoreMouseEnterProperty =
      DependencyProperty.RegisterAttached("IgnoreMouseEnter", typeof(bool), typeof(MouseHelper), new PropertyMetadata(default(bool)));

    public static void SetIgnoreMouseEnter(UIElement element, bool value)
    {
      element.SetValue(IgnoreMouseEnterProperty, value);
    }

    public static bool GetIgnoreMouseEnter(UIElement element)
    {
      return (bool)element.GetValue(IgnoreMouseEnterProperty);
    }
    #endregion

    #region mouse move

    public static readonly DependencyProperty MouseMoveProperty =
      DependencyProperty.RegisterAttached("MouseMove", typeof (Action<Point>), typeof (MouseHelper), new PropertyMetadata(default(Action<Point>), MouseMoveChanged));

    private static void MouseMoveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null)
        return;

      Action<Point> oldValue = dependencyPropertyChangedEventArgs.OldValue as Action<Point>;
      if (oldValue != null)
        uiElement.MouseMove -= UiElementOnMouseMove;

      Action<Point> newValue = dependencyPropertyChangedEventArgs.NewValue as Action<Point>;
      if (newValue != null)
        uiElement.MouseMove += UiElementOnMouseMove;
    
    }

    private static void UiElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      Point point = mouseEventArgs.GetPosition((IInputElement) sender);
      GetMouseMove((UIElement) sender).Invoke(point);
    }

    public static void SetMouseMove(UIElement element, Action<Point> value)
    {
      element.SetValue(MouseMoveProperty, value);
    }

    public static Action<Point> GetMouseMove(UIElement element)
    {
      return (Action<Point>) element.GetValue(MouseMoveProperty);
    }
    #endregion
  }
}
