using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Commands;
using SharpE.MvvmTools.Properties;

namespace SharpE.BaseEditors.Image
{
  class ImageViewerViewModel : IEditor
  {
    private UIElement m_view;
    private IFileViewModel m_file;
    private readonly IEnumerable<string> m_supportedFiles = new List<string> { ".png" };
    private System.Windows.Controls.Image m_image;
    private double m_zoom;
    private readonly IObservableCollection<IMenuItemViewModel> m_menuItems = null;
    private readonly ManualCommand m_rotateLeftCommand;
    private readonly ManualCommand m_rotateRightCommand;
    private readonly ManualCommand m_applySizeCommand;
    private int m_width;
    private int m_height;
    private Thickness m_cropMargin = new Thickness(300,0,0,0);
    private Size m_cropSize = new Size(100,100);
    private readonly ManualCommand m_mouseDownCommand;
    private readonly ManualCommand m_mouseUpCommand;
    private readonly ManualCommand m_cropCommand;
    private bool m_mouseIsDown;
    private Point m_mousePos;
    private Point m_startPos;

    public ImageViewerViewModel()
    {
      m_rotateLeftCommand = new ManualCommand(RotateLeft);
      m_rotateRightCommand = new ManualCommand(RotateRight);
      m_applySizeCommand = new ManualCommand(Resize);
      m_mouseDownCommand = new ManualCommand(MouseDown);
      m_mouseUpCommand = new ManualCommand(MouseUp);
      m_cropCommand = new ManualCommand(Crop);
    }

    private void Crop()
    {
      double screenScale = m_width/m_image.ActualWidth;
      UpdateImage(Rotation.Rotate0, null, null, new Int32Rect((int) (m_cropMargin.Left * screenScale), (int) (m_cropMargin.Top * screenScale), (int) (m_cropSize.Width *screenScale), (int) (m_cropSize.Height *screenScale)));
      SendImageToFile();
    }

    private void MouseUp()
    {
      m_mouseIsDown = false;
    }

    private void MouseDown()
    {
      m_mouseIsDown = true;
      m_startPos = m_mousePos;
      CropMargin = new Thickness(m_startPos.X, m_startPos.Y, 0, 0);
      CropSize = new Size(0,0);
    }

    private void Resize()
    {
      UpdateImage(Rotation.Rotate0, m_width, m_height);
      SendImageToFile();
    }

    private void RotateRight()
    {
      UpdateImage(Rotation.Rotate90);
      SendImageToFile();
    }

    private void RotateLeft()
    {

      UpdateImage(Rotation.Rotate270);
      SendImageToFile();
    }

    public string Name
    {
      get { return "ImageViewerViewModel"; }
    }

    private void SendImageToFile()
    {
      BitmapEncoder bitmapEncoder;
      switch (m_file.Exstension)
      {
        case ".png":
          bitmapEncoder = new PngBitmapEncoder();
          break;
        case ".jpg":
        case ".jpeg":
          bitmapEncoder = new JpegBitmapEncoder();
          break;
        case ".gif":
          bitmapEncoder = new GifBitmapEncoder();
          break;
        case ".bmp":
          bitmapEncoder = new BmpBitmapEncoder();
          break;
        case ".tif":
          bitmapEncoder = new TiffBitmapEncoder();
          break;
        default:
          return;
      }
      MemoryStream stream = new MemoryStream();
      bitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)m_image.Source));
      bitmapEncoder.Save(stream);
      m_file.SetContent(stream);
    }

    public IFileViewModel Settings
    {
      get { return null; }
    }

    public UIElement View
    {
      get
      {
        if (m_view == null)
        {
          Image = new System.Windows.Controls.Image { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left, Stretch = Stretch.None };
          m_view = new ImageViewerView { DataContext = this };
          if (m_file != null)
            UpdateImage();
        }
        return m_view;
      }
    }

    private void UpdateImage(Rotation rotation = Rotation.Rotate0, int? width = null, int? height = null, Int32Rect? cropRect = null)
    {
      BitmapImage bitmap = new BitmapImage();
      bitmap.BeginInit();
      bitmap.Rotation = rotation;
      bitmap.StreamSource = m_file.GetContent<Stream>();
      if (width.HasValue)
        bitmap.DecodePixelWidth = width.Value;
      if (height.HasValue)
        bitmap.DecodePixelHeight = height.Value;
      bitmap.EndInit();
      BitmapSource source = bitmap;
      if (cropRect.HasValue)
        source = new CroppedBitmap(bitmap, cropRect.Value);
      Width = source.PixelWidth;
      Height = source.PixelHeight;
      CropMargin = new Thickness(0, 0, 0, 0);
      CropSize = new Size(0,0);
      m_image.Source = source;
    }

    public IFileViewModel File
    {
      get { return m_file; }
      set
      {
        if (m_file != null)
        {
          m_file.SetTag("zoom", m_zoom);
          m_file.ContentChanged -= FileOnContentChanged;
        }
        m_file = value;
        if (m_file != null)
        {
          double? zoom = m_file.GetTag("zoom") as double?;
          Zoom = zoom.HasValue ? zoom.Value : 1;
          if (m_view != null)
            UpdateImage();
          m_file.ContentChanged += FileOnContentChanged;
        }
        OnPropertyChanged();
      }
    }

    private void FileOnContentChanged(IFileViewModel fileViewModel)
    {
      UpdateImage();
    }

    public IEnumerable<string> SupportedFiles
    {
      get { return m_supportedFiles; }
    }

    public IEditor CreateNew()
    {
      return new ImageViewerViewModel();
    }

    public IObservableCollection<IMenuItemViewModel> MenuItems
    {
      get { return m_menuItems; }
    }

    public System.Windows.Controls.Image Image
    {
      get { return m_image; }
      set { m_image = value; }
    }

    public double Zoom
    {
      get { return m_zoom; }
      set
      {
        if (value < 0.1)
          value = 0.1;
        if (value > 5)
          value = 5;
        if (value.Equals(m_zoom)) return;
        m_zoom = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand RotateLeftCommand
    {
      get { return m_rotateLeftCommand; }
    }

    public ManualCommand RotateRightCommand
    {
      get { return m_rotateRightCommand; }
    }

    public int Width
    {
      get { return m_width; }
      set
      {
        if (value == m_width) return;
        m_width = value;
        OnPropertyChanged();
      }
    }

    public int Height
    {
      get { return m_height; }
      set
      {
        if (value == m_height) return;
        m_height = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand ApplySizeCommand
    {
      get { return m_applySizeCommand; }
    }

    public Action<int> ScrollAction
    {
      get { return Scroll; }
    }

    public Thickness CropMargin
    {
      get { return m_cropMargin; }
      set
      {
        if (value.Equals(m_cropMargin)) return;
        m_cropMargin = value;
        OnPropertyChanged();
      }
    }

    public Size CropSize
    {
      get { return m_cropSize; }
      set
      {
        if (value.Equals(m_cropSize)) return;
        m_cropSize = value;
        OnPropertyChanged();
      }
    }

    public ManualCommand MouseDownCommand
    {
      get { return m_mouseDownCommand; }
    }

    public ManualCommand MouseUpCommand
    {
      get { return m_mouseUpCommand; }
    }

    public Action<Point> MouseMoveAction
    {
      get { return MouseMove; }
    }

    public ManualCommand CropCommand
    {
      get { return m_cropCommand; }
    }

    private void MouseMove(Point obj)
    {
      m_mousePos = obj;
      if (m_mouseIsDown)
      {
        double left = Math.Min(m_mousePos.X, m_startPos.X);
        double top = Math.Min(m_mousePos.Y, m_startPos.Y);
        double right = Math.Max(m_mousePos.X, m_startPos.X);
        double bottom = Math.Max(m_mousePos.Y, m_startPos.Y);
        CropMargin = new Thickness(left, top, 0, 0);
        CropSize = new Size(right - left, bottom - top);
      }
    }

    private void Scroll(int delta)
    {
      Zoom += delta/1200.0;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
