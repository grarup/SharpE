using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SharpE.MvvmTools.Controls
{
  public class DirectItemsControl : FrameworkElement
  {
    private DataTemplate m_dataTemplate;
    private Panel m_panel;
    private readonly Dictionary<object, FrameworkElement> m_elements = new Dictionary<object, FrameworkElement>();

    public static readonly DependencyProperty ItemsSourceProperty =
      DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(DirectItemsControl), new FrameworkPropertyMetadata(default(IEnumerable), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      DirectItemsControl directItemsControl = (DirectItemsControl)dependencyObject;

      INotifyCollectionChanged oldObservable = dependencyPropertyChangedEventArgs.OldValue as INotifyCollectionChanged;
      if (oldObservable != null)
        oldObservable.CollectionChanged -= directItemsControl.CollectionChanged;
      INotifyCollectionChanged newObservable = dependencyPropertyChangedEventArgs.NewValue as INotifyCollectionChanged;
      if (newObservable != null)
        newObservable.CollectionChanged += directItemsControl.CollectionChanged;
      directItemsControl.Init();

    }

    private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (object item in e.NewItems)
          {
            FrameworkElement frameworkElement = m_dataTemplate.LoadContent() as FrameworkElement;
            if (frameworkElement == null) continue;
            frameworkElement.DataContext = item;
            m_panel.Children.Add(frameworkElement);
            m_elements.Add(item, frameworkElement);
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (object item in e.OldItems)
          {
            if (!m_elements.ContainsKey(item)) continue;
            m_panel.Children.Remove(m_elements[item]);
            m_elements.Remove(item);
          }
          break;
        case NotifyCollectionChangedAction.Replace:
          foreach (object item in e.OldItems)
          {
            if (!m_elements.ContainsKey(item)) continue;
            m_panel.Children.Remove(m_elements[item]);
            m_elements.Remove(item);
          }
          foreach (object item in e.NewItems)
          {
            FrameworkElement frameworkElement = m_dataTemplate.LoadContent() as FrameworkElement;
            if (frameworkElement == null) continue;
            frameworkElement.DataContext = item;
            m_panel.Children.Add(frameworkElement);
            m_elements.Add(item, frameworkElement);
          }
          break;
        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Reset:
          Init();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }


    public IEnumerable ItemsSource
    {
      get { return (IEnumerable)GetValue(ItemsSourceProperty); }
      set { SetValue(ItemsSourceProperty, value); }
    }

    public DataTemplate DataTemplate
    {
      get { return m_dataTemplate; }
      set
      {
        m_dataTemplate = value;
        Init();
      }
    }

    private void Init()
    {
      List<FrameworkElement> itemsToRemove = m_elements.Values.ToList();
      m_elements.Clear();
      if (ItemsSource == null || m_dataTemplate == null || m_panel == null) return;
      foreach (object item in ItemsSource)
      {
        FrameworkElement frameworkElement = m_dataTemplate.LoadContent() as FrameworkElement;
        if (frameworkElement == null) continue;
        frameworkElement.DataContext = item;
        m_elements.Add(item, frameworkElement);
      }
      List<FrameworkElement> itemsToAdd = m_elements.Values.ToList();
      try
      {
        foreach (FrameworkElement frameworkElement in itemsToRemove)
          m_panel.Children.Remove(frameworkElement);
        foreach (FrameworkElement frameworkElement in itemsToAdd)
          m_panel.Children.Add(frameworkElement);
      }
      catch (Exception)
      {
        Action update = () =>
        {
          foreach (FrameworkElement frameworkElement in itemsToRemove)
            m_panel.Children.Remove(frameworkElement);
          foreach (FrameworkElement frameworkElement in itemsToAdd)
            m_panel.Children.Remove(frameworkElement);
          foreach (FrameworkElement frameworkElement in itemsToAdd)
            m_panel.Children.Add(frameworkElement);
        };
        m_panel.Dispatcher.BeginInvoke(update);
      }
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
      if (m_panel != null)
      {
        foreach (FrameworkElement frameworkElement in m_elements.Values)
        {
          m_panel.Children.Remove(frameworkElement);
        }
      }
      m_elements.Clear();
      base.OnVisualParentChanged(oldParent);
      m_panel = VisualParent as Panel;
      Init();
    }
  }
}
 