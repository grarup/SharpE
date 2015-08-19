using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SharpE.MvvmTools.Helpers
{
  public class TreeViewHelper
  {
    public static object M_none = new object();

    public static readonly DependencyProperty SelectedItemProperty =
      DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(TreeViewHelper), new PropertyMetadata(M_none, PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      TreeView treeView = dependencyObject as TreeView;
      if (treeView == null) return;
      treeView.SelectedItemChanged -= TreeViewOnSelectedItemChanged;
      treeView.SelectedItemChanged += TreeViewOnSelectedItemChanged;
      if (treeView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
      {
        if (dependencyPropertyChangedEventArgs.NewValue != null)
        {
          TreeViewItem treeViewItem = FindTreeViewItem(treeView.ItemContainerGenerator,
                                                       dependencyPropertyChangedEventArgs.NewValue);
          if (treeViewItem != null)
            treeViewItem.IsSelected = true;
          else
            Deselect(treeView.ItemContainerGenerator);
        }
        else
        {
          Deselect(treeView.ItemContainerGenerator);
        }
      }
      else
      {
        EventHandler statusChangedHandler = null;
        statusChangedHandler = (sender, eventArgs) =>
        {
          if (treeView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
          {
            treeView.ItemContainerGenerator.StatusChanged -= statusChangedHandler;
            PropertyChangedCallback(dependencyObject, dependencyPropertyChangedEventArgs);
          }
        };

        treeView.ItemContainerGenerator.StatusChanged += statusChangedHandler;
      }
    }

    private static bool Deselect(ItemContainerGenerator itemContainerGenerator)
    {
      if (itemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return false;
      foreach (object item in itemContainerGenerator.Items)
      {
        TreeViewItem treeViewItem = (TreeViewItem)itemContainerGenerator.ContainerFromItem(item);
        if (treeViewItem.IsSelected)
        {
          treeViewItem.IsSelected = false;
          return true;
        }
        bool retVal = Deselect(treeViewItem.ItemContainerGenerator);
        if (retVal)
          return true;
      }
      return false;
    }

    private static TreeViewItem FindTreeViewItem(ItemContainerGenerator itemContainerGenerator, object dataContext)
    {
      if (itemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return null;
      if (itemContainerGenerator.Items.Contains(dataContext))
        return (TreeViewItem)itemContainerGenerator.ContainerFromItem(dataContext);
      foreach (object item in itemContainerGenerator.Items)
      {
        TreeViewItem treeViewItem = (TreeViewItem)itemContainerGenerator.ContainerFromItem(item);
        TreeViewItem retVal = FindTreeViewItem(treeViewItem.ItemContainerGenerator, dataContext);
        if (retVal != null)
          return retVal;
      }
      return null;
    }

    private static void TreeViewOnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
    {
      SetSelectedItem((TreeView)sender, routedPropertyChangedEventArgs.NewValue);
    }

    public static void SetSelectedItem(TreeView element, object value)
    {
      element.SetValue(SelectedItemProperty, value);
    }

    public static object GetSelectedItem(TreeView element)
    {
      return element.GetValue(SelectedItemProperty);
    }

    public static readonly DependencyProperty SelectWithRightClickProperty =
      DependencyProperty.RegisterAttached("SelectWithRightClick", typeof (bool), typeof (TreeViewHelper), new PropertyMetadata(default(bool), SelectWithRightClickPropertyChangedCallback));

    private static void SelectWithRightClickPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      TreeView treeView = dependencyObject as TreeView;
      if (treeView == null)
        throw new ArgumentException("Only works on treeview");

      if (dependencyPropertyChangedEventArgs.OldValue != null && (bool) dependencyPropertyChangedEventArgs.OldValue)
      {
        treeView.PreviewMouseRightButtonDown -= TreeViewOnPreviewMouseRightButtonDown;
      }

      if (dependencyPropertyChangedEventArgs.NewValue != null && (bool)dependencyPropertyChangedEventArgs.NewValue)
      {
        treeView.PreviewMouseRightButtonDown += TreeViewOnPreviewMouseRightButtonDown;
      }
    }

    private static void TreeViewOnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      UIElement uiElement = Mouse.DirectlyOver as UIElement;
      while (!(uiElement is TreeViewItem) && uiElement != null)
      {
        uiElement = (UIElement)VisualTreeHelper.GetParent(uiElement);
      }
      if (uiElement != null)
        ((TreeViewItem)uiElement).IsSelected = true;
    }

    public static void SetSelectWithRightClick(UIElement element, bool value)
    {
      element.SetValue(SelectWithRightClickProperty, value);
    }

    public static bool GetSelectWithRightClick(UIElement element)
    {
      return (bool) element.GetValue(SelectWithRightClickProperty);
    }

    public static readonly DependencyProperty BringSelecteIntoViewProperty = DependencyProperty.RegisterAttached(
      "BringSelecteIntoView", typeof(bool), typeof(TreeViewHelper), new PropertyMetadata(default(bool), BreingSelectedIntoViewChangedCallback));

    private static void BreingSelectedIntoViewChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      TreeView treeView = dependencyObject as TreeView;
      if (treeView == null)
        throw new ArgumentException("only works on treeviews");

      if ((bool) dependencyPropertyChangedEventArgs.OldValue)
        treeView.SelectedItemChanged -= TreeViewOnSelectedItemChanged2;

      if ((bool) dependencyPropertyChangedEventArgs.NewValue)
        treeView.SelectedItemChanged += TreeViewOnSelectedItemChanged2;
    }

    private static void TreeViewOnSelectedItemChanged2(object sender, RoutedPropertyChangedEventArgs<object> routedPropertyChangedEventArgs)
    {
      TreeView treeView = sender as TreeView;
      if (treeView == null)
        return;

      TreeViewItem treeViewItem = FindTreeViewItem(treeView.ItemContainerGenerator, treeView.SelectedItem);
      if (treeViewItem != null)
        treeViewItem.BringIntoView();
    }

    public static void SetBringSelecteIntoView(DependencyObject element, bool value)
    {
      element.SetValue(BringSelecteIntoViewProperty, value);
    }

    public static bool GetBringSelecteIntoView(DependencyObject element)
    {
      return (bool)element.GetValue(BringSelecteIntoViewProperty);
    }

  }
}
