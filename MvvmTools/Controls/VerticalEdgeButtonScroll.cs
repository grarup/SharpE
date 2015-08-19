using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Common.MedRxCommonControls.Controls
{
  public class VerticalEdgeButtonScroll : ContentControl
  {
    private Button m_upButton;
    private Button m_downButton;
    private ContentControl m_contentControl;
    private double m_offset;
    private FrameworkElement m_content;
    private double m_scrollArea;
    private Border m_viewArea;
    private double m_stepSize = 25;
    private DoubleAnimation m_doubleAnimation;
    private Storyboard m_storyboard;
    private TranslateTransform m_translateTransform;
    private GradientStop m_topGradientStop;
    private GradientStop m_buttomGradientStop;

    static VerticalEdgeButtonScroll()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(VerticalEdgeButtonScroll), new FrameworkPropertyMetadata(typeof(VerticalEdgeButtonScroll)));
    }

    public VerticalEdgeButtonScroll()
    {
      SizeChanged += OnSizeChanged;
      if (!DesignerProperties.GetIsInDesignMode(this))
        Loaded += OnLoaded;
      MouseWheel += OnMouseWheel;
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
    {
      Offset += (mouseWheelEventArgs.Delta/120)*m_stepSize;
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      if (m_contentControl == null) return;
      TransformGroup transformGroup = new TransformGroup();
      m_translateTransform = new TranslateTransform(0, 0);
      string translateTransformName = "t" + m_translateTransform.GetHashCode();
      RegisterName(translateTransformName, m_translateTransform);
      transformGroup.Children.Add(m_translateTransform);
      m_contentControl.RenderTransform = transformGroup;
      m_doubleAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(125)));
      m_storyboard = new Storyboard();
      Resources.Add("s" + m_storyboard.GetHashCode(), m_storyboard);
      m_storyboard.Children.Add(m_doubleAnimation);
      Storyboard.SetTargetName(m_doubleAnimation, translateTransformName);
      Storyboard.SetTargetProperty(m_doubleAnimation, new PropertyPath(TranslateTransform.YProperty));
    }


    private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
    {
      Update();
    }

    private double Offset
    {
      get { return m_offset; }
      set
      {
        if (m_offset == value) return;
        if (value > 0)
          value = 0;
        if (value < -m_scrollArea)
          value = -m_scrollArea;
        m_offset = value;
        m_doubleAnimation.From = m_translateTransform.Y;
        m_doubleAnimation.To = m_offset;
        m_storyboard.Begin(this);
        m_upButton.IsEnabled = m_offset < 0;
        m_downButton.IsEnabled = m_offset > -m_scrollArea;
        m_topGradientStop.Color = m_offset < 0 ? Colors.Transparent : Colors.Black;
        m_buttomGradientStop.Color = m_offset > -m_scrollArea ? Colors.Transparent : Colors.Black;
      }
    }

    public double StepSize
    {
      get { return m_stepSize; }
      set { m_stepSize = value; }
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
      base.OnContentChanged(oldContent, newContent);

      if (m_content != null)
        m_content.SizeChanged -= ContentOnSizeChanged;

      m_content = newContent as FrameworkElement;
      if (m_content != null)
        m_content.SizeChanged += ContentOnSizeChanged;

      if (m_contentControl != null)
        m_contentControl.Content = newContent;

      Update();
    }

    private void Update()
    {
      if (m_upButton == null || m_downButton == null || m_contentControl == null)
        return;


      if (m_content == null || ActualHeight - m_content.ActualHeight >= 0)
      {
        Offset = 0;
        m_upButton.Visibility = Visibility.Collapsed;
        m_downButton.Visibility = Visibility.Collapsed;
        m_scrollArea = 0;
      }
      else
      {
        m_upButton.Visibility = Visibility.Visible;
        m_downButton.Visibility = Visibility.Visible;
        m_scrollArea = m_content.ActualHeight - m_viewArea.ActualHeight;
        if (m_offset < -m_scrollArea)
          Offset = -m_scrollArea;
        m_upButton.IsEnabled = m_offset < 0;
        m_downButton.IsEnabled = m_offset > -m_scrollArea;
      }
      m_topGradientStop.Color = m_offset < 0 ? Colors.Transparent : Colors.Black;
      m_buttomGradientStop.Color = m_offset > -m_scrollArea ? Colors.Transparent : Colors.Black;
    }

    private void ContentOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
    {
      Update();
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      m_upButton = (Button)GetTemplateChild("PART_UpButton");
      m_downButton = (Button)GetTemplateChild("PART_DownButton");
      m_contentControl = (ContentControl)GetTemplateChild("PART_Content");
      m_viewArea = (Border) GetTemplateChild("PART_ViewArea");

      if (m_upButton == null || m_downButton == null || m_contentControl == null || m_viewArea == null)
        throw new Exception("Templet must contain PART_ViewArea, PART_Content, PART_UpButton and PART_DownButton");

      m_upButton.Click += UpButtonOnClick;
      m_downButton.Click += DownButtonOnClick;
      m_contentControl.Content = Content;

      GradientStopCollection gradientStopCollection = new GradientStopCollection();
      m_topGradientStop = new GradientStop(Colors.Transparent, 0);
      gradientStopCollection.Add(m_topGradientStop);
      gradientStopCollection.Add(new GradientStop(Colors.Black, 0.02));
      gradientStopCollection.Add(new GradientStop(Colors.Black, 0.98));
      m_buttomGradientStop = new GradientStop(Colors.Transparent, 1);
      gradientStopCollection.Add(m_buttomGradientStop);

      m_viewArea.OpacityMask = new LinearGradientBrush(gradientStopCollection) { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };

      Update();

      if (IsLoaded)
        OnLoaded(null, null);
    }

    private void DownButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      Offset -= m_stepSize;
    }

    private void UpButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      Offset += m_stepSize;
    }
  }
}
