using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SharpE.MvvmTools.Helpers
{
  public class DragHelper
  {
    private readonly object m_data;
    private bool m_complete;

    public DragHelper(object data)
    {
      m_data = data;
    }

    public object Data
    {
      get { return m_data; }
    }

    public bool Complete
    {
      get { return m_complete; }
      set { m_complete = value; }
    }

    public static readonly DependencyProperty DragDataProperty =
      DependencyProperty.RegisterAttached("DragData", typeof(object), typeof(DragHelper), new FrameworkPropertyMetadata(default(object), DragDataPropertyChanged));

    private static void DragDataPropertyChanged(DependencyObject dependency, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      UIElement uiElement = dependency as UIElement;
      if (uiElement == null) return;
      uiElement.MouseMove += UiElementOnMouseMove;
    }

    private static void UiElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      UIElement uiElement = sender as UIElement;
      if (uiElement != null && mouseEventArgs.LeftButton == MouseButtonState.Pressed && GetEnableDrag(uiElement))
      {
        uiElement.Dispatcher.BeginInvoke(DispatcherPriority.Normal , new Action<UIElement>(DoDrag), uiElement);
      }
    }

    private static void DoDrag(UIElement uiElement)
    {
      object data = GetDragData(uiElement);
      if (data == null || !GetEnableDrag(uiElement)) return;
      try
      {
        DragHelper dragHelper = new DragHelper(data);
        DataObject dataObject = new DataObject(typeof(object), dragHelper);
        Action<object, bool> completAction = GetDropComplete(uiElement);
        Action<object> dragAction = GetDragAction(uiElement);
        if (dragAction != null)
          Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, dragAction, data);
        DragDrop.DoDragDrop(uiElement, dataObject, GetDragDroppEffects(uiElement));
        if (completAction != null)
          Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, completAction, data, dragHelper.Complete);
      }
      catch
      {
        //Remove Com exception
      }
    }

    public static void SetDragData(UIElement element, object value)
    {
      element.SetValue(DragDataProperty, value);
    }

    public static object GetDragData(UIElement element)
    {
      return element.GetValue(DragDataProperty);
    }

    public static readonly DependencyProperty DragDroppEffectsProperty =
      DependencyProperty.RegisterAttached("DragDroppEffects", typeof(DragDropEffects), typeof(DragHelper), new PropertyMetadata(default(DragDropEffects)));

    public static void SetDragDroppEffects(UIElement element, DragDropEffects value)
    {
      element.SetValue(DragDroppEffectsProperty, value);
    }

    public static DragDropEffects GetDragDroppEffects(UIElement element)
    {
      return (DragDropEffects)element.GetValue(DragDroppEffectsProperty);
    }

    public static readonly DependencyProperty EnableDragProperty =
      DependencyProperty.RegisterAttached("EnableDrag", typeof (bool), typeof (DragHelper), new PropertyMetadata(true, PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      
    }

    public static void SetEnableDrag(UIElement element, bool value)
    {
      element.SetValue(EnableDragProperty, value);
    }

    public static bool GetEnableDrag(UIElement element)
    {
      return (bool) element.GetValue(EnableDragProperty);
    }

    public static readonly DependencyProperty DragActionProperty =
      DependencyProperty.RegisterAttached("DragAction", typeof (Action<object>), typeof (DragHelper), new PropertyMetadata(default(Action<object>)));

    public static void SetDragAction(UIElement element, Action<object> value)
    {
      element.SetValue(DragActionProperty, value);
    }

    public static Action<object> GetDragAction(UIElement element)
    {
      return (Action<object>) element.GetValue(DragActionProperty);
    }

    public static readonly DependencyProperty DropCompleteProperty =
      DependencyProperty.RegisterAttached("DropComplete", typeof (Action<object, bool>), typeof (DragHelper), new PropertyMetadata(default(Action<object, bool>), Property2ChangedCallback));

    private static void Property2ChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      
    }

    public static void SetDropComplete(UIElement element, Action<object, bool> value)
    {
      element.SetValue(DropCompleteProperty, value);
    }

    public static Action<object, bool> GetDropComplete(UIElement element)
    {
      return (Action<object, bool>) element.GetValue(DropCompleteProperty);
    }
  }
}
