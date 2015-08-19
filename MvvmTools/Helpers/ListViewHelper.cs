using System.Windows;
using System.Windows.Controls;

namespace SharpE.MvvmTools.Helpers
{
  public class ListViewHelper
  {
    public static readonly DependencyProperty AutoScrollToSelectedItemProperty =
      DependencyProperty.RegisterAttached("AutoScrollToSelectedItem", typeof(bool), typeof(ListViewHelper), new PropertyMetadata(default(bool), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      ListView listBox = dependencyObject as ListView;
      if (listBox == null) return;
      if ((bool)dependencyPropertyChangedEventArgs.NewValue)
      {
        listBox.SelectionChanged += ListBoxOnSelectionChanged;
      }
      else
      {
        listBox.SelectionChanged -= ListBoxOnSelectionChanged;
      }
    }

    private static void ListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
      ListView listBox = sender as ListView;
      if (listBox == null || listBox.SelectedItem == null) return;
      listBox.ScrollIntoView(listBox.SelectedItem);
    }

    public static void SetAutoScrollToSelectedItem(ListView element, bool value)
    {
      element.SetValue(AutoScrollToSelectedItemProperty, value);
    }

    public static bool GetAutoScrollToSelectedItem(ListView element)
    {
      return (bool)element.GetValue(AutoScrollToSelectedItemProperty);
    }
  }
}
