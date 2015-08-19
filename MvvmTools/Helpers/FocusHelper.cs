using System;
using System.Windows;
using System.Windows.Threading;

namespace SharpE.MvvmTools.Helpers
{
  public class FocusHelper
  {
    public static readonly DependencyProperty FocusTagProperty = DependencyProperty.RegisterAttached(
      "FocusTag", typeof(object), typeof(FocusHelper), new PropertyMetadata(default(object)));

    public static void SetFocusTag(DependencyObject element, object value)
    {
      element.SetValue(FocusTagProperty, value);
    }

    public static object GetFocusTag(DependencyObject element)
    {
      return element.GetValue(FocusTagProperty);
    }

    public static readonly DependencyProperty CurrentFocusTagProperty = DependencyProperty.RegisterAttached(
      "CurrentFocusTag", typeof(object), typeof(FocusHelper), new PropertyMetadata(default(object), CurrentFocusTagChanged));

    private static void CurrentFocusTagChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null)
        return;
      if (dependencyPropertyChangedEventArgs.NewValue == null)
        return;

      if (GetFocusTag(dependencyObject).Equals(dependencyPropertyChangedEventArgs.NewValue))
      { 
        uiElement.Dispatcher.BeginInvoke(new Action(() => uiElement.Focus()), DispatcherPriority.Input);
      }
    }

    public static void SetCurrentFocusTag(DependencyObject element, object value)
    {
      element.SetValue(CurrentFocusTagProperty, value);
    }

    public static object GetCurrentFocusTag(DependencyObject element)
    {
      return element.GetValue(CurrentFocusTagProperty);
    }

    public static readonly DependencyProperty IsFocusProperty =
      DependencyProperty.RegisterAttached("IsFocus", typeof (bool), typeof (FocusHelper), new PropertyMetadata(default(bool), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null) return;

      if ((bool) dependencyPropertyChangedEventArgs.NewValue)
      {
        uiElement.Dispatcher.BeginInvoke(new Action(() => uiElement.Focus()), DispatcherPriority.Input);
      }
    }

    public static void SetIsFocus(UIElement element, bool value)
    {
      element.SetValue(IsFocusProperty, value);
    }

    public static bool GetIsFocus(UIElement element)
    {
      return (bool) element.GetValue(IsFocusProperty);
    }

    public static readonly DependencyProperty HasFocusProperty = DependencyProperty.RegisterAttached(
      "HasFocus", typeof (Action<bool>), typeof (FocusHelper), new PropertyMetadata(default(Action<bool>), HasFocusPropertyChangedCallback));

    private static void HasFocusPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null) return;

      Action<bool> oldAction = dependencyPropertyChangedEventArgs.OldValue as Action<bool>;
      if (oldAction != null)
      {
        uiElement.GotFocus -= UiElementOnGotFocus;
        uiElement.LostFocus -= UiElementOnLostFocus;
      }

      Action<bool> newAction = dependencyPropertyChangedEventArgs.NewValue as Action<bool>;
      if (newAction != null)
      {
        uiElement.GotFocus += UiElementOnGotFocus;
        uiElement.LostFocus += UiElementOnLostFocus;
      }
    }

    private static void UiElementOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
    {
      Action<bool> action = GetHasFocus((DependencyObject) sender);
      if (action != null)
        action(false);
    }

    private static void UiElementOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
    {
      Action<bool> action = GetHasFocus((DependencyObject)sender);
      if (action != null)
        action(true);
    }

    public static void SetHasFocus(DependencyObject element, Action<bool> value)
    {
      element.SetValue(HasFocusProperty, value);
    }

    public static Action<bool> GetHasFocus(DependencyObject element)
    {
      return (Action<bool>) element.GetValue(HasFocusProperty);
    }
  }
}
