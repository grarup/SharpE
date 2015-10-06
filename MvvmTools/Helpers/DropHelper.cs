using System;
using System.Windows;
using System.Windows.Input;

namespace SharpE.MvvmTools.Helpers
{
  public class DropHelper
  {
    public static readonly DependencyProperty DropActionProperty =
      DependencyProperty.RegisterAttached("DropAction", typeof (Action<object, DragDropEffects>), typeof (DropHelper), new FrameworkPropertyMetadata(default(ICommand), DropCommandPropertyChanged));

    private static void DropCommandPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      UIElement uiElement = dependencyObject as UIElement;
      if (uiElement == null) return;
      uiElement.Drop += UiElementOnDrop;
    }

    private static void UiElementOnDrop(object sender, DragEventArgs dragEventArgs)
    {
      UIElement uiElement = sender as UIElement;
      if (uiElement == null) return;
      Action<object, DragDropEffects> action = GetDropAction(uiElement);
      if (action == null) return;
      DragHelper dragHelper = dragEventArgs.Data.GetData(typeof (object)) as DragHelper;
      if (dragHelper == null)
        action(dragEventArgs.Data.GetData(typeof(object)), dragEventArgs.AllowedEffects);
      else
      {
        dragHelper.Complete = true;
        action(dragHelper.Data, dragEventArgs.AllowedEffects);
      }
    }

    public static void SetDropAction(UIElement element, Action<object, DragDropEffects> value)
    {
      element.SetValue(DropActionProperty, value);
    }

    public static Action<object, DragDropEffects> GetDropAction(UIElement element)
    {
      return (Action<object, DragDropEffects>)element.GetValue(DropActionProperty);
    }

  }
}
