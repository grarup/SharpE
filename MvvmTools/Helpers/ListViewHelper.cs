using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SharpE.MvvmTools.Helpers
{
  public static class ListViewHelper
  {
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(out POINT lpPoint);


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

    public static readonly DependencyProperty EnableDragHoverProperty =
      DependencyProperty.RegisterAttached("EnableDragHover", typeof(UIElement), typeof(ListViewHelper), new PropertyMetadata(default(UIElement), OnEnableDragHoverChangedCallback));

    private static void OnEnableDragHoverChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      ListView listView = dependencyObject as ListView;
      if (listView == null) return;

      UIElement oldValue = dependencyPropertyChangedEventArgs.OldValue as UIElement;
      if (oldValue != null)
      {
        oldValue.DragOver -= UiElementOnDragOver;
        oldValue.PreviewDragLeave -= UiElementOnDragLeave;
        SetListView(oldValue, null);
      }

      UIElement newValue = dependencyPropertyChangedEventArgs.NewValue as UIElement;
      if (newValue != null)
      {
        newValue.DragOver += UiElementOnDragOver;
        newValue.PreviewDragLeave += UiElementOnDragLeave;
        SetListView(newValue, listView);
      }
    }

    private static readonly DependencyProperty ListViewProperty =
      DependencyProperty.RegisterAttached("ListView", typeof(ListView), typeof(ListViewHelper), new PropertyMetadata(default(ListView)));

    public static void SetListView(UIElement element, ListView value)
    {
      element.SetValue(ListViewProperty, value);
    }

    public static ListView GetListView(UIElement element)
    {
      return (ListView)element.GetValue(ListViewProperty);
    }

    private static readonly DependencyProperty OrignalSelectedItemProperty =
      DependencyProperty.RegisterAttached("OrignalSelectedItem", typeof(object), typeof(ListViewHelper), new PropertyMetadata(default(object)));

    public static void SetOrignalSelectedItem(UIElement element, object value)
    {
      element.SetValue(OrignalSelectedItemProperty, value);
    }

    public static object GetOrignalSelectedItem(UIElement element)
    {
      return (object)element.GetValue(OrignalSelectedItemProperty);
    }

    private static bool IsChildOf(DependencyObject child, DependencyObject parrent)
    {
      DependencyObject newChild = VisualTreeHelper.GetParent(child);
      if (newChild == null)
        return false;
      return Equals(parrent, newChild) || IsChildOf(newChild, parrent);
    }

    private static void UiElementOnDragLeave(object sender, DragEventArgs dragEventArgs)
    {
      ListView listView = GetListView((UIElement)sender);
      if (listView == null) return;

      POINT p;
      GetCursorPos(out p);

      Point point = new Point(p.x, p.y);

      point = ((UIElement) sender).PointFromScreen(point);
      Rect bounds = VisualTreeHelper.GetDescendantBounds((Visual)sender);
      if (bounds.Contains(point))
        return;

      object data = dragEventArgs.Data.GetData(typeof(object));
      DragHelper dragHelper = data as DragHelper;
      if (dragHelper != null)
        data = dragHelper.Data;

      IList list = listView.ItemsSource as IList ?? listView.Items;
      list.Remove(data);
      if (dragHelper != null)
        dragHelper.Complete = false;

      object originalSelectedItem = GetOrignalSelectedItem(listView);
      if (originalSelectedItem != null)
        listView.SelectedItem = originalSelectedItem;
      else
      {
        if (listView.SelectedItem == null)
          listView.SelectedItem = list.Count > 0 ? list[0] : null;
      }
    }

    private static void UiElementOnDragOver(object sender, DragEventArgs dragEventArgs)
    {
      ListView listView = GetListView((UIElement)sender);
      if (listView == null) return;

      object data = dragEventArgs.Data.GetData(typeof(object));
      DragHelper dragHelper = data as DragHelper;
      if (dragHelper != null)
        data = dragHelper.Data;

      object itemUnderMouse = listView.GetDataFromPoint(dragEventArgs);
      if (itemUnderMouse != null && itemUnderMouse == data)
        return;

      IList list = listView.ItemsSource as IList ?? listView.Items;

      if (listView.SelectedItem != data)
        SetOrignalSelectedItem(listView, listView.SelectedItem);

      if (itemUnderMouse == null)
      {
        if (!list.Contains(data))
          list.Add(data);
        listView.SelectedItem = data;
        if (dragHelper != null)
          dragHelper.Complete = true;
        return;
      }

      ListViewItem listViewItem = (ListViewItem) listView.ItemContainerGenerator.ContainerFromItem(itemUnderMouse);
      Rect itemUnderRect = VisualTreeHelper.GetDescendantBounds(listViewItem);
      Rect dataItemRect = VisualTreeHelper.GetDescendantBounds((Visual) listView.ItemContainerGenerator.ContainerFromItem(data));
      Rect testRect = new Rect(itemUnderRect.Location, dataItemRect.Size);
      if (testRect.Contains(dragEventArgs.GetPosition(listViewItem)))
        return;

      int index = list.IndexOf(itemUnderMouse);
      int indexData = list.IndexOf(data);

      if (indexData != -1)
        list.Remove(data);

      list.Insert(index, data);
      if (dragHelper != null)
        dragHelper.Complete = true;

      listView.SelectedItem = data;
    }

    public static void SetEnableDragHover(ListView listView, UIElement value)
    {
      listView.SetValue(EnableDragHoverProperty, value);
    }

    public static UIElement GetEnableDragHover(ListView listView)
    {
      return (UIElement)listView.GetValue(EnableDragHoverProperty);
    }

    public static object GetDataFromPoint(this ListView listView, DragEventArgs dragEventArgs)
    {
      if (listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
        return null;

      for (int index = 0; index < listView.Items.Count; index++)
      {
        ListViewItem listViewItem = listView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        Rect bounds = VisualTreeHelper.GetDescendantBounds(listViewItem);
        Point position = dragEventArgs.GetPosition(listViewItem);
        if (bounds.Contains(position))
          return listView.Items[index];
      }

      return null;
    }
  }
}
