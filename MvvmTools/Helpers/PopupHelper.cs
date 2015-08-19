using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dgs.MvvmTools.Helpers.Implementations
{
  public enum PopupAction
  {
    Toggle,
    Open,
    Close,
  }

  public class PopupHelper
  {
    public static readonly DependencyProperty PopupActionProperty =
      DependencyProperty.RegisterAttached("PopupAction", typeof (PopupAction), typeof (PopupHelper), new PropertyMetadata(default(PopupAction)));

    public static void SetPopupAction(Button button, PopupAction value)
    {
      button.SetValue(PopupActionProperty, value);
    }

    public static PopupAction GetPopupAction(Button button)
    {
      return (PopupAction) button.GetValue(PopupActionProperty);
    }

    private static void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      Popup popup = GetPopupToControl((Button) sender);
      if (popup == null) return;
      switch (GetPopupAction((Button) sender))
      {
        case PopupAction.Toggle:
          popup.IsOpen = !popup.IsOpen;
          break;
        case PopupAction.Open:
          popup.IsOpen = true;
          break;
        case PopupAction.Close:
          popup.IsOpen = false;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public static readonly DependencyProperty PopupToControlProperty =
      DependencyProperty.RegisterAttached("PopupToControl", typeof (Popup), typeof (PopupHelper), new PropertyMetadata(default(Popup), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      Button button = dependencyObject as Button;
      if (button == null) return;
      button.Click -= ButtonOnClick;
      Popup popup = dependencyPropertyChangedEventArgs.NewValue as Popup;
      if (popup == null) return;
      button.Click += ButtonOnClick;
    }

    public static void SetPopupToControl(Button button, Popup value)
    {
      button.SetValue(PopupToControlProperty, value);
    }

    public static Popup GetPopupToControl(Button button)
    {
      return (Popup) button.GetValue(PopupToControlProperty);
    }


    public static readonly DependencyProperty PopupToMoveProperty =
      DependencyProperty.RegisterAttached("PopupToMove", typeof (Popup), typeof (PopupHelper), new PropertyMetadata(default(Popup), PopupToMovePropertyChangedCallback));

    private static void PopupToMovePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      Thumb thumb = dependencyObject as Thumb;
      if (thumb == null) return;
      thumb.DragDelta -= ThumbOnDragDelta;
      thumb.DragDelta += ThumbOnDragDelta;
    }

    private static void ThumbOnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
    {
      Popup popup = GetPopupToMove((Thumb) sender);
      popup.HorizontalOffset += dragDeltaEventArgs.HorizontalChange;
      popup.VerticalOffset += dragDeltaEventArgs.VerticalChange;
    }

    public static void SetPopupToMove(Thumb thumb, Popup value)
    {
      thumb.SetValue(PopupToMoveProperty, value);
    }

    public static Popup GetPopupToMove(Thumb thumb)
    {
      return (Popup) thumb.GetValue(PopupToMoveProperty);
    }
  }
}
