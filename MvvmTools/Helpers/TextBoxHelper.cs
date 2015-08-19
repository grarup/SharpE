using System;
using System.Windows;
using System.Windows.Controls;

namespace SharpE.MvvmTools.Helpers
{
  public static class TextBoxHelper
  {
    public static readonly DependencyProperty SelectAllOnFocusProperty =
      DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof (bool), typeof (TextBoxHelper), new PropertyMetadata(default(bool), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      TextBox textBox = dependencyObject as TextBox;
      if (textBox == null)
        throw new ArgumentException("Only works on textboxes");

      if ((bool) dependencyPropertyChangedEventArgs.OldValue)
      {
        textBox.GotFocus -= TextBoxOnGotFocus;
      }

      if ((bool)dependencyPropertyChangedEventArgs.NewValue)
      {
        textBox.GotFocus += TextBoxOnGotFocus;
      }
    }

    private static void TextBoxOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
    {
      TextBox textBox = sender as TextBox;
      if (textBox == null || textBox.Text.Length == 0) return;
     
      if (GetSelectionWithOutExstension(textBox))
      {
        int index = textBox.Text.LastIndexOf('.');
        textBox.Select(0, index < 0 ? textBox.Text.Length : index);
      }
      else
        textBox.Select(0, textBox.Text.Length);
    }

    public static void SetSelectAllOnFocus(UIElement element, bool value)
    {
      element.SetValue(SelectAllOnFocusProperty, value);
    }

    public static bool GetSelectAllOnFocus(UIElement element)
    {
      return (bool) element.GetValue(SelectAllOnFocusProperty);
    }

    public static readonly DependencyProperty SelectionWithOutExstensionProperty =
      DependencyProperty.RegisterAttached("SelectionWithOutExstension", typeof (bool), typeof (TextBoxHelper), new PropertyMetadata(default(bool)));

    public static void SetSelectionWithOutExstension(UIElement element, bool value)
    {
      element.SetValue(SelectionWithOutExstensionProperty, value);
    }

    public static bool GetSelectionWithOutExstension(UIElement element)
    {
      return (bool) element.GetValue(SelectionWithOutExstensionProperty);
    }

    public static readonly DependencyProperty CaretIndexProperty = DependencyProperty.RegisterAttached(
      "CaretIndex", typeof(int), typeof(TextBoxHelper), new PropertyMetadata(default(int), CaretIndexPropertyChangedCallback));

    private static void CaretIndexPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      TextBox textBox = dependencyObject as TextBox;
      if (textBox == null) return;

      textBox.CaretIndex = (int) dependencyPropertyChangedEventArgs.NewValue;
    }

    public static void SetCaretIndex(DependencyObject element, int value)
    {
      element.SetValue(CaretIndexProperty, value);
    }

    public static int GetCaretIndex(DependencyObject element)
    {
      return (int) element.GetValue(CaretIndexProperty);
    }
  }
}
