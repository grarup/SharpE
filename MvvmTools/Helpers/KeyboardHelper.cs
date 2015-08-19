using System;
using System.Windows;
using System.Windows.Input;

namespace SharpE.MvvmTools.Helpers
{
  public class KeyboardHelper
  {
    public static readonly DependencyProperty KeyDownPreviewProperty =
      DependencyProperty.RegisterAttached("KeyDownPreview", typeof(Func<Key, bool>), typeof(KeyboardHelper), new UIPropertyMetadata(default(Func<Key, bool>), KeyDownPreviewPropertyChangedCallback));

    private static void KeyDownPreviewPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = (UIElement) dependencyObject;
      
      if (dependencyPropertyChangedEventArgs.OldValue != null)
        uiElement.PreviewKeyDown -= OnPreviewKeyDown;

      if (dependencyPropertyChangedEventArgs.NewValue != null)
        uiElement.PreviewKeyDown += OnPreviewKeyDown;
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
    {
      keyEventArgs.Handled |= GetKeyDownPreview((UIElement) sender)(keyEventArgs.Key);
    }

    public static void SetKeyDownPreview(UIElement element, Func<Key, bool> value)
    {
      element.SetValue(KeyDownPreviewProperty, value);
    }

    public static Func<Key, bool> GetKeyDownPreview(UIElement element)
    {
      return (Func<Key, bool>)element.GetValue(KeyDownPreviewProperty);
    }

    public static readonly DependencyProperty KeyUpPreviewProperty =
      DependencyProperty.RegisterAttached("KeyUpPreview", typeof(Func<Key, bool>), typeof(KeyboardHelper), new FrameworkPropertyMetadata(default(Func<Key, bool>), KeyDownPropertyChangedCallback));

    private static void KeyDownPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = (UIElement)dependencyObject;

      if (dependencyPropertyChangedEventArgs.OldValue != null)
        uiElement.PreviewKeyUp -= OnPreviewKeyUp;

      if (dependencyPropertyChangedEventArgs.NewValue != null)
        uiElement.PreviewKeyUp += OnPreviewKeyUp;
      
    }

    private static void OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
      e.Handled |= GetKeyUpPreview((UIElement) sender)(e.Key);
    }

    public static void SetKeyUpPreview(UIElement element, Func<Key, bool> value)
    {
      element.SetValue(KeyUpPreviewProperty, value);
    }

    public static Func<Key, bool> GetKeyUpPreview(UIElement element)
    {
      return (Func<Key, bool>)element.GetValue(KeyUpPreviewProperty);
    }

    public static readonly DependencyProperty IgnoreKeyPressProperty =
      DependencyProperty.RegisterAttached("IgnoreKeyPress", typeof (bool), typeof (KeyboardHelper), new FrameworkPropertyMetadata(default(bool), IgnoreKeyPressPropertyChangedCallback));

    private static void IgnoreKeyPressPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = (UIElement)dependencyObject;

      if ((bool) dependencyPropertyChangedEventArgs.NewValue)
        uiElement.PreviewKeyDown += IgnoreKeyPress;
      else
        uiElement.PreviewKeyDown -= IgnoreKeyPress;
    }

    private static void IgnoreKeyPress(object sender, KeyEventArgs e)
    {
      e.Handled = true;
    }

    public static void SetIgnoreKeyPress(UIElement element, bool value)
    {
      element.SetValue(IgnoreKeyPressProperty, value);
    }

    public static bool GetIgnoreKeyPress(UIElement element)
    {
      return (bool) element.GetValue(IgnoreKeyPressProperty);
    }
  }
}
