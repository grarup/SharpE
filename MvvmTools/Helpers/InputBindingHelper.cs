using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Microsoft.CSharp.RuntimeBinder;

namespace SharpE.MvvmTools.Helpers
{
  public static class InputBindingHelper
  {
    public static readonly DependencyProperty InputBindingSourceProperty =
      DependencyProperty.RegisterAttached("InputBindingSource", typeof(IEnumerable<KeyBinding>), typeof(InputBindingHelper), new PropertyMetadata(default(IEnumerable<KeyBinding>), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null)
        throw new RuntimeBinderException("source need to be of type UIElement");

      uiElement.InputBindings.Clear();
      IEnumerable<KeyBinding> newList = dependencyPropertyChangedEventArgs.NewValue as IEnumerable<KeyBinding>;
      if (newList != null)
      {
        foreach (KeyBinding keyBinding in newList)
          uiElement.InputBindings.Add(keyBinding);
      }
    }

    public static void SetInputBindingSource(UIElement element, IEnumerable<KeyBinding> value)
    {
      element.SetValue(InputBindingSourceProperty, value);
    }

    public static IEnumerable<KeyBinding> GetInputBindingSource(UIElement element)
    {
      return (IEnumerable<KeyBinding>)element.GetValue(InputBindingSourceProperty);
    }
  }
}
