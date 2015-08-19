using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using SharpE.MvvmTools.Helpers;

namespace SharpE.MvvmTools.Controls
{
  public class StyledWindow : Window
  {
    private FrameworkElement m_caption;
    private Point m_startPosition;
    private double m_startWidth;
    private double m_startHeight;
    private bool m_isResizeing;
    private FrameworkElement m_topLeftBorder;
    private FrameworkElement m_topCenterBorder;
    private FrameworkElement m_topRightBorder;
    private FrameworkElement m_middelLeftBorder;
    private FrameworkElement m_middelRightBorder;
    private FrameworkElement m_bottomLeftBorder;
    private FrameworkElement m_bottomCenterBorder;
    private FrameworkElement m_bottomRightrBorder;
    private Button m_minimizeButton;
    private Button m_maxemizeButton;
    private Button m_closeButton;
    private IntPtr m_handler;
    private ContentControl m_xmalIconContainer;
    private Point? m_dragPosition;

    [DllImport("user32")]
    private static extern int RegisterWindowMessage(string message);
    private static readonly int s_newStart = RegisterWindowMessage("WM_NEW_START");

    public static readonly DependencyProperty XamlIconProperty =
      DependencyProperty.Register("XamlIcon", typeof(FrameworkElement), typeof(StyledWindow), new PropertyMetadata(default(FrameworkElement), PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      StyledWindow styledWindow = dependencyObject as StyledWindow;
      if (styledWindow == null || styledWindow.m_xmalIconContainer == null) return;
      styledWindow.m_xmalIconContainer.Content = dependencyPropertyChangedEventArgs.NewValue;
    }

    public FrameworkElement XamlIcon
    {
      get { return (FrameworkElement)GetValue(XamlIconProperty); }
      set { SetValue(XamlIconProperty, value); }
    }

    public static readonly DependencyProperty KeyDownHandlerProperty =
      DependencyProperty.Register("KeyDownHandler", typeof(Func<Key, bool>), typeof(StyledWindow), new PropertyMetadata(default(Func<Key, bool>)));

    public Func<Key, bool> KeyDownHandler
    {
      get { return (Func<Key, bool>)GetValue(KeyDownHandlerProperty); }
      set { SetValue(KeyDownHandlerProperty, value); }
    }

    public static readonly DependencyProperty KeyUpHandlerProperty =
      DependencyProperty.Register("KeyUpHandler", typeof(Func<Key, bool>), typeof(StyledWindow), new PropertyMetadata(default(ICommand)));


    public Func<Key, bool> KeyUpHandler
    {
      get { return (Func<Key, bool>)GetValue(KeyUpHandlerProperty); }
      set { SetValue(KeyUpHandlerProperty, value); }
    }

    public StyledWindow()
    {
      SourceInitialized += OnSourceInitialized;
      Style = (Style)TryFindResource("WindowStyle");
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
      if (KeyDownHandler != null && !e.IsRepeat)
        e.Handled = KeyDownHandler.Invoke(e.Key);
      base.OnPreviewKeyDown(e);
    }

    protected override void OnPreviewKeyUp(KeyEventArgs e)
    {
      if (KeyUpHandler != null && !e.IsRepeat)
        e.Handled = KeyUpHandler.Invoke(e.Key);
      base.OnPreviewKeyUp(e);
    }

    private void OnSourceInitialized(object sender, EventArgs eventArgs)
    {
      m_handler = new WindowInteropHelper(this).Handle;
      HwndSource hwndSource = HwndSource.FromHwnd(m_handler);
      if (hwndSource != null) hwndSource.AddHook(WinProc);
    }

    private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
      if (msg == (int)WmNofications.Getminmaxinfo)
      {
        WmGetMinMaxInfo(hwnd, lparam);
        handled = true;
      }
      else if (msg == s_newStart)
      {

      }
      return IntPtr.Zero;
    }

    private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
    {
      MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

      const int monitorDefaulttonearest = 0x00000002;
      IntPtr monitor = NativeMethods.MonitorFromWindow(hwnd, monitorDefaulttonearest);

      if (monitor != IntPtr.Zero)
      {
        MONITORINFO monitorInfo = new MONITORINFO();
        NativeMethods.GetMonitorInfo(monitor, monitorInfo);

        RECT rcWorkArea = monitorInfo.rcWork;
        RECT rcMonitorArea = monitorInfo.rcMonitor;

        mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
        mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
        mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
        mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
      }

      Marshal.StructureToPtr(mmi, lParam, true);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      m_topLeftBorder = (FrameworkElement)GetTemplateChild("PART_TopLeftBorder");
      m_topCenterBorder = (FrameworkElement)GetTemplateChild("PART_TopCenterBorder");
      m_topRightBorder = (FrameworkElement)GetTemplateChild("PART_TopRightBorder");
      m_middelLeftBorder = (FrameworkElement)GetTemplateChild("PART_MiddelLeftBorder");
      m_middelRightBorder = (FrameworkElement)GetTemplateChild("PART_MiddelRightBorder");
      m_bottomLeftBorder = (FrameworkElement)GetTemplateChild("PART_BottomLeftBorder");
      m_bottomCenterBorder = (FrameworkElement)GetTemplateChild("PART_BottomCenterBorder");
      m_bottomRightrBorder = (FrameworkElement)GetTemplateChild("PART_BottomRightBorder");


      if (m_topLeftBorder != null)
        m_topLeftBorder.MouseEnter += (sender, args) => ((Border)sender).Cursor = CanResize() ? Cursors.SizeNWSE : Cursors.Arrow;
      if (m_topCenterBorder != null)
        m_topCenterBorder.MouseEnter += (sender, args) => ((Border)sender).Cursor = CanResize() ? Cursors.SizeNS : Cursors.Arrow;
      if (m_topRightBorder != null)
        m_topRightBorder.MouseEnter += (sender, args) => ((Border)sender).Cursor = CanResize() ? Cursors.SizeNESW : Cursors.Arrow;
      if (m_middelLeftBorder != null)
        m_middelLeftBorder.MouseEnter += (sender, args) => ((Border)sender).Cursor = CanResize() ? Cursors.SizeWE : Cursors.Arrow;
      if (m_middelRightBorder != null)
        m_middelRightBorder.MouseEnter += (sender, args) => ((Border)sender).Cursor = CanResize() ? Cursors.SizeWE : Cursors.Arrow;
      if (m_bottomLeftBorder != null)
        m_bottomLeftBorder.MouseEnter += (sender, args) => ((Border)sender).Cursor = CanResize() ? Cursors.SizeNESW : Cursors.Arrow;
      if (m_bottomCenterBorder != null)
        m_bottomCenterBorder.MouseEnter += (sender, args) => ((Border)sender).Cursor = CanResize() ? Cursors.SizeNS : Cursors.Arrow;
      if (m_bottomRightrBorder != null)
        m_bottomRightrBorder.MouseEnter += (sender, args) => ((Border)sender).Cursor = CanResize() ? Cursors.SizeNWSE : Cursors.Arrow;

      m_topLeftBorder.MouseLeave += (sender, args) => ((Border)sender).Cursor = Cursors.Arrow;
      m_topCenterBorder.MouseLeave += (sender, args) => ((Border)sender).Cursor = Cursors.Arrow;
      m_topRightBorder.MouseLeave += (sender, args) => ((Border)sender).Cursor = Cursors.Arrow;
      m_middelLeftBorder.MouseLeave += (sender, args) => ((Border)sender).Cursor = Cursors.Arrow;
      m_middelRightBorder.MouseLeave += (sender, args) => ((Border)sender).Cursor = Cursors.Arrow;
      m_bottomLeftBorder.MouseLeave += (sender, args) => ((Border)sender).Cursor = Cursors.Arrow;
      m_bottomCenterBorder.MouseLeave += (sender, args) => ((Border)sender).Cursor = Cursors.Arrow;
      m_bottomRightrBorder.MouseLeave += (sender, args) => ((Border)sender).Cursor = Cursors.Arrow;

      m_topLeftBorder.MouseDown += BorderOnMouseDown;
      m_topCenterBorder.MouseDown += BorderOnMouseDown;
      m_topRightBorder.MouseDown += BorderOnMouseDown;
      m_middelLeftBorder.MouseDown += BorderOnMouseDown;
      m_middelRightBorder.MouseDown += BorderOnMouseDown;
      m_bottomLeftBorder.MouseDown += BorderOnMouseDown;
      m_bottomCenterBorder.MouseDown += BorderOnMouseDown;
      m_bottomRightrBorder.MouseDown += BorderOnMouseDown;

      m_topLeftBorder.MouseMove += TopLeftBorderOnMouseMove;
      m_topCenterBorder.MouseMove += TopCenterBorderOnMouseMove;
      m_topRightBorder.MouseMove += TopRightBorderOnMouseMove;
      m_middelLeftBorder.MouseMove += MiddelLeftBorderOnMouseMove;
      m_middelRightBorder.MouseMove += MiddelRightBorderOnMouseMove;
      m_bottomLeftBorder.MouseMove += BottomLeftBorderOnMouseMove;
      m_bottomCenterBorder.MouseMove += BottomCenterBorderOnMouseMove;
      m_bottomRightrBorder.MouseMove += BottomRightrBorderOnMouseMove;

      m_topLeftBorder.MouseUp += BorderOnMouseUp;
      m_topCenterBorder.MouseUp += BorderOnMouseUp;
      m_topRightBorder.MouseUp += BorderOnMouseUp;
      m_middelLeftBorder.MouseUp += BorderOnMouseUp;
      m_middelRightBorder.MouseUp += BorderOnMouseUp;
      m_bottomLeftBorder.MouseUp += BorderOnMouseUp;
      m_bottomCenterBorder.MouseUp += BorderOnMouseUp;
      m_bottomRightrBorder.MouseUp += BorderOnMouseUp;

      m_minimizeButton = (Button)GetTemplateChild("PART_MinimizeButton");
      m_maxemizeButton = (Button)GetTemplateChild("PART_MaxemizeButton");
      m_closeButton = (Button)GetTemplateChild("PART_CloseButton");

      if (m_minimizeButton != null)
        m_minimizeButton.Click += MinimizeButtonOnClick;
      if (m_maxemizeButton != null)
        m_maxemizeButton.Click += MaxemizeButtonOnClick;
      if (m_closeButton != null)
        m_closeButton.Click += CloseButtonOnClick;

      m_caption = (FrameworkElement)GetTemplateChild("PART_Caption");
      if (m_caption != null)
      {
        m_caption.MouseDown += CaptionOnMouseDown;
        m_caption.MouseMove += OnMouseMove;
      }

      m_xmalIconContainer = (ContentControl)GetTemplateChild("PART_XamlIcon");
      if (m_xmalIconContainer != null)
        m_xmalIconContainer.Content = XamlIcon;
    }

    private void OnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      if (m_dragPosition != null && mouseEventArgs.LeftButton == MouseButtonState.Pressed)
      {
        Point position = mouseEventArgs.GetPosition(this);
        if (position.X != m_dragPosition.Value.X || position.Y != m_dragPosition.Value.X)
        {
          m_dragPosition = null;
          WindowState = WindowState.Normal;
          Left = position.X - (Width / 2);
          Top = position.Y;
          DragMove();
        }
      }
    }

    private void BottomRightrBorderOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      if (!m_isResizeing) return;
      Point point = mouseEventArgs.GetPosition(this);
      double offsetY = point.Y - m_startPosition.Y;
      Height = m_startHeight + offsetY < MinHeight ? MinHeight : m_startHeight + offsetY;
      double offsetX = point.X - m_startPosition.X;
      Width = m_startWidth + offsetX < MinWidth ? MinWidth : m_startWidth + offsetX;
    }

    private void BottomCenterBorderOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      if (!m_isResizeing) return;
      Point point = mouseEventArgs.GetPosition(this);
      double offsetY = point.Y - m_startPosition.Y;
      Height = m_startHeight + offsetY < MinHeight ? MinHeight : m_startHeight + offsetY;
    }

    private void BottomLeftBorderOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      if (!m_isResizeing) return;
      Point point = mouseEventArgs.GetPosition(this);
      double offsetY = point.Y - m_startPosition.Y;
      Height = m_startHeight + offsetY < MinHeight ? MinHeight : m_startHeight + offsetY;
      double offsetX = point.X - m_startPosition.X;
      double changeX = Width - (m_startWidth - offsetX);
      if (Width - changeX < MinWidth)
      {
        changeX = Width - MinWidth;
      }
      Left += changeX;
      Width -= changeX;
      m_startPosition.Offset(-changeX, 0);
    }

    private void MiddelRightBorderOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      if (!m_isResizeing) return;
      Point point = mouseEventArgs.GetPosition(this);
      double offsetX = point.X - m_startPosition.X;
      Width = m_startWidth + offsetX < MinWidth ? MinWidth : m_startWidth + offsetX;
    }

    private void MiddelLeftBorderOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      if (!m_isResizeing) return;
      Point point = mouseEventArgs.GetPosition(this);
      double offsetX = point.X - m_startPosition.X;
      double changeX = Width - (m_startWidth - offsetX);
      if (Width - changeX < MinWidth)
      {
        changeX = Width - MinWidth;
      }
      Left += changeX;
      Width -= changeX;
      m_startPosition.Offset(-changeX, 0);
    }

    private void TopRightBorderOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      if (!m_isResizeing) return;
      Point point = mouseEventArgs.GetPosition(this);
      double offsetY = point.Y - m_startPosition.Y;
      double changeY = Height - (m_startHeight - offsetY);
      if (Height - changeY < MinHeight)
      {
        changeY = Height - MinHeight;
      }
      Top += changeY;
      Height -= changeY;
      double offsetX = point.X - m_startPosition.X;
      Width = m_startWidth + offsetX < MinWidth ? MinWidth : m_startWidth + offsetX;
      m_startPosition.Offset(0, -changeY);
    }

    private void TopCenterBorderOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      if (!m_isResizeing) return;
      Point point = mouseEventArgs.GetPosition(this);
      double offsetY = point.Y - m_startPosition.Y;
      double changeY = Height - (m_startHeight - offsetY);
      if (Height - changeY < MinHeight)
      {
        changeY = Height - MinHeight;
      }
      Top += changeY;
      Height -= changeY;
      m_startPosition.Offset(0, -changeY);
    }

    private void BorderOnMouseUp(object sender, MouseButtonEventArgs args)
    {
      ((Border)sender).ReleaseMouseCapture();
      m_isResizeing = false;
    }

    private void TopLeftBorderOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      if (!m_isResizeing) return;
      Point point = mouseEventArgs.GetPosition(this);
      double offsetY = point.Y - m_startPosition.Y;
      double changeY = Height - (m_startHeight - offsetY);
      if (Height - changeY < MinHeight)
      {
        changeY = Height - MinHeight;
      }
      Top += changeY;
      Height -= changeY;
      double offsetX = point.X - m_startPosition.X;
      double changeX = Width - (m_startWidth - offsetX);
      if (Width - changeX < MinWidth)
      {
        changeX = Width - MinWidth;
      }
      Left += changeX;
      Width -= changeX;
      m_startPosition.Offset(-changeX, -changeY);
    }

    private void BorderOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      m_isResizeing = true;
      if (!CanResize()) return;
      Border border = (Border)sender;
      border.CaptureMouse();
      m_startPosition = mouseButtonEventArgs.GetPosition(this);
      m_startWidth = Width;
      m_startHeight = Height;
    }

    private bool CanResize()
    {
      return WindowState != WindowState.Maximized && ResizeMode == ResizeMode.CanResize;
    }

    private void CaptionOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      if (mouseButtonEventArgs.ClickCount == 2 && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
      {
        MaxemizeButtonOnClick(null, null);
        return;
      }
      if (mouseButtonEventArgs.ChangedButton == MouseButton.Left)
      {
        if (WindowState != WindowState.Maximized)
        {
          if (mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
            DragMove();
        }
        else
        {
          m_dragPosition = mouseButtonEventArgs.GetPosition(this);
        }
      }
    }

    private void CloseButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      Close();
    }

    private void MaxemizeButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (ResizeMode != ResizeMode.NoResize)
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void MinimizeButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (ResizeMode != ResizeMode.NoResize)
        WindowState = WindowState.Minimized;
    }
  }
}
