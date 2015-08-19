using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Common.MedRxCommonControls.Controls
{
  public class HorizontalEdgeButtonScroll : ContentControl
  {
    private Button m_leftButton;
    private Button m_rightButton;
    private ContentControl m_contentControl;
    private double m_offset;
    private FrameworkElement m_content;
    private double m_scrollArea;
    private Border m_viewArea;
    private double m_stepSize = 25;
    private Storyboard m_storyboard;
    private GradientStop m_topGradientStop;
    private GradientStop m_buttomGradientStop;
    private ThicknessAnimation m_thicknessAnimation;

    static HorizontalEdgeButtonScroll()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(HorizontalEdgeButtonScroll), new FrameworkPropertyMetadata(typeof(HorizontalEdgeButtonScroll)));
    }

    public HorizontalEdgeButtonScroll()
    {
      if (!DesignerProperties.GetIsInDesignMode(this))
        Loaded += OnLoaded;
      MouseWheel += OnMouseWheel;
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
    {
      Offset += (mouseWheelEventArgs.Delta / 120) * m_stepSize;
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      m_thicknessAnimation = new ThicknessAnimation(new Thickness(0), new Duration(TimeSpan.FromMilliseconds(125)));
      m_storyboard = new Storyboard();
      string s = "s" + m_storyboard.GetHashCode();
      Resources.Add(s, m_storyboard);
      m_storyboard.Children.Add(m_thicknessAnimation);
      Storyboard.SetTarget(m_thicknessAnimation, m_contentControl);
      Storyboard.SetTargetProperty(m_thicknessAnimation, new PropertyPath(MarginProperty));
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
        m_thicknessAnimation.From = m_contentControl.Margin;
        m_thicknessAnimation.To = new Thickness(m_offset, 0, 0, 0);
        m_storyboard.Begin(this);
        m_leftButton.IsEnabled = m_offset < 0;
        m_rightButton.IsEnabled = m_offset > -m_scrollArea;
        //m_topGradientStop.Color = m_offset < 0 ? Colors.Transparent : Colors.Black;
        //m_buttomGradientStop.Color = m_offset > -m_scrollArea ? Colors.Transparent : Colors.Black;
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
      if (m_leftButton == null || m_rightButton == null || m_contentControl == null)
        return;


      if (ActualWidth - m_content.ActualWidth >= 0)
      {
        Offset = 0;
        m_leftButton.Visibility = Visibility.Collapsed;
        m_rightButton.Visibility = Visibility.Collapsed;
        m_scrollArea = 0;
      }
      else
      {
        m_leftButton.Visibility = Visibility.Visible;
        m_rightButton.Visibility = Visibility.Visible;
        m_scrollArea = m_content.ActualWidth - m_viewArea.ActualWidth;
        if (m_offset < -m_scrollArea)
          Offset = -m_scrollArea;
        m_leftButton.IsEnabled = m_offset < 0;
        m_rightButton.IsEnabled = m_offset > -m_scrollArea;
      }
      //m_topGradientStop.Color = m_offset < 0 ? Colors.Transparent : Colors.Black;
      //m_buttomGradientStop.Color = m_offset > -m_scrollArea ? Colors.Transparent : Colors.Black;
    }

    private void ContentOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
    {
      Update();
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      m_leftButton = (Button)GetTemplateChild("PART_LeftButton");
      m_rightButton = (Button)GetTemplateChild("PART_RightButton");
      m_contentControl = (ContentControl)GetTemplateChild("PART_Content");
      m_viewArea = (Border)GetTemplateChild("PART_ViewArea");

      if (m_leftButton == null || m_rightButton == null || m_contentControl == null || m_viewArea == null)
        throw new Exception("Templet must contain PART_ViewArea, PART_Content, PART_UpButton and PART_DownButton");

      m_leftButton.Click += LeftButtonOnClick;
      m_rightButton.Click += RightButtonOnClick;
      m_contentControl.Content = Content;

      //GradientStopCollection gradientStopCollection = new GradientStopCollection();
      //m_topGradientStop = new GradientStop(Colors.Transparent, 0);
      //gradientStopCollection.Add(m_topGradientStop);
      //gradientStopCollection.Add(new GradientStop(Colors.Black, 0.01));
      //gradientStopCollection.Add(new GradientStop(Colors.Black, 0.99));
      //m_buttomGradientStop = new GradientStop(Colors.Transparent, 1);
      //gradientStopCollection.Add(m_buttomGradientStop);

      //m_viewArea.OpacityMask = new LinearGradientBrush(gradientStopCollection) { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0)};
      m_viewArea.SizeChanged += OnSizeChanged;

      if (m_thicknessAnimation != null)
        Storyboard.SetTarget(m_thicknessAnimation, m_contentControl);

      Update();
    }

    private void RightButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      Offset -= m_stepSize;
    }

    private void LeftButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      Offset += m_stepSize;
    }
  }
}
