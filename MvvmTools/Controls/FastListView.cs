using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SharpE.MvvmTools.Controls
{
  public class FastListView : FrameworkElement
  {
    private double m_itemHeight;
    private Size m_clipSize;
    private readonly List<object> m_list = new List<object>();

    public static readonly DependencyProperty ItemRenderProperty = DependencyProperty.Register(
      "ItemRender", typeof (IItemRender), typeof (FastListView), new PropertyMetadata(new SimpleItemRender(), Invalidate));

    public IItemRender ItemRender
    {
      get { return (IItemRender) GetValue(ItemRenderProperty); }
      set { SetValue(ItemRenderProperty, value); }
    }

    public static readonly DependencyProperty ViewportSizeProperty =
      DependencyProperty.Register("ViewportSize", typeof (double), typeof (FastListView), new PropertyMetadata(default(double)));

    public double ViewportSize
    {
      get { return (double) GetValue(ViewportSizeProperty); }
      set { SetValue(ViewportSizeProperty, value); }
    }

    public static readonly DependencyProperty ScrollMaxProperty =
      DependencyProperty.Register("ScrollMax", typeof (double), typeof (FastListView), new PropertyMetadata(default(double)));

    public double ScrollMax
    {
      get { return (double) GetValue(ScrollMaxProperty); }
      set { SetValue(ScrollMaxProperty, value); }
    }

    public static readonly DependencyProperty ScrollValueProperty =
      DependencyProperty.Register("ScrollValue", typeof (double), typeof (FastListView), new PropertyMetadata(default(double), Invalidate));

    public double ScrollValue
    {
      get { return (double) GetValue(ScrollValueProperty); }
      set { SetValue(ScrollValueProperty, value); }
    }

    public static readonly DependencyProperty SelectedIndexProperty =
      DependencyProperty.Register("SelectedIndex", typeof (int), typeof (FastListView), new PropertyMetadata(default(int), IndexChanged));

    private static void Invalidate(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      FastListView fastListView = dependencyObject as FastListView;
      if (fastListView == null)
        return;
      fastListView.InvalidateVisual();
    }

    private static void IndexChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      FastListView fastListView = dependencyObject as FastListView;
      if (fastListView == null)
        return;
      double pos = fastListView.SelectedIndex*fastListView.m_itemHeight;
      if (pos < fastListView.ScrollValue)
        fastListView.ScrollValue = pos;
      if (pos + (fastListView.m_itemHeight * 2) > (fastListView.ScrollValue + fastListView.m_clipSize.Height))
        fastListView.ScrollValue = pos - fastListView.m_clipSize.Height + 2 * fastListView.m_itemHeight;
      fastListView.InvalidateVisual();
    }

    public int SelectedIndex
    {
      get { return (int) GetValue(SelectedIndexProperty); }
      set { SetValue(SelectedIndexProperty, value); }
    }

    public static readonly DependencyProperty ItemSourceProperty =
      DependencyProperty.Register("ItemSource", typeof (IEnumerable), typeof (FastListView), new PropertyMetadata(default(IEnumerable), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      FastListView fastListView = dependencyObject as FastListView;
      if (fastListView == null)
        return;
      object item = fastListView.SelectedIndex < fastListView.m_list.Count ? fastListView.m_list[fastListView.SelectedIndex] : null;
      fastListView.m_list.Clear();
      IEnumerable enumerable = dependencyPropertyChangedEventArgs.NewValue as IEnumerable;
      if (enumerable == null)
        return;
      IEnumerator enumerator = enumerable.GetEnumerator();
      while (enumerator.MoveNext())
        fastListView.m_list.Add(enumerator.Current);
      if (item != null)
      {
        fastListView.SelectedIndex = fastListView.m_list.IndexOf(item);
        if (fastListView.SelectedIndex == -1)
          fastListView.SelectedIndex = 0;
      }
      fastListView.InvalidateMeasure();
      INotifyCollectionChanged notifyCollectionChanged = dependencyPropertyChangedEventArgs.NewValue as INotifyCollectionChanged;
      if (notifyCollectionChanged != null)
      {
        notifyCollectionChanged.CollectionChanged += fastListView.CollectionChanged;
      }
    }

    private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      object item = SelectedIndex < m_list.Count ? m_list[SelectedIndex] : null;
      m_list.Clear();
      IEnumerator enumerator = ItemSource.GetEnumerator();
      while (enumerator.MoveNext())
        m_list.Add(enumerator.Current);
      if (item != null)
      {
        int newIndex = m_list.IndexOf(item);
        SelectedIndex = newIndex == -1 ? 0 : newIndex;
      }
      InvalidateMeasure();
      InvalidateVisual();
    }

    public IEnumerable ItemSource
    {
      get { return (IEnumerable) GetValue(ItemSourceProperty); }
      set { SetValue(ItemSourceProperty, value); }
    }


    protected override Geometry GetLayoutClip(Size layoutSlotSize)
    {
      m_clipSize = layoutSlotSize;
      return base.GetLayoutClip(layoutSlotSize);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      m_itemHeight = ItemRender.ItemHeight;
      ScrollMax = Math.Max(0, m_itemHeight* (m_list == null? 0 : m_list.Count) - m_clipSize.Height + 2 * m_itemHeight);
      ViewportSize = m_clipSize.Height;
      return new Size(Width, ScrollMax );
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
      double newScrollValue = ScrollValue - (m_itemHeight * (e.Delta/120.0));
      if (newScrollValue < 0)
        newScrollValue = 0;
      if (newScrollValue > ScrollMax)
        newScrollValue = ScrollMax;
      ScrollValue = newScrollValue;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      drawingContext.PushClip(GetLayoutClip(m_clipSize));
      Rect rect = new Rect(new Point(0,0), m_clipSize);
      drawingContext.DrawRectangle(Brushes.Transparent, null, rect);

      int index = ((int) (ScrollValue/m_itemHeight));
      Point position = new Point(5,5);
      while (position.Y < m_clipSize.Height && index < m_list.Count)
      {
        if (SelectedIndex == index)
        {
          SolidColorBrush mySolidColorBrush = new SolidColorBrush { Color = Color.FromArgb(0xFF, 0x00, 0x80, 0xC0) };
          Rect myRect = new Rect(new Point(position.X - 3, position.Y) , new Size(Width - 4, m_itemHeight));
          drawingContext.DrawRectangle(mySolidColorBrush, null, myRect);
        }
        ItemRender.Render(drawingContext, position, m_list[index]);
        position.Offset(0, m_itemHeight);
        index++;
      }
      drawingContext.Pop();
    }
  }
}
