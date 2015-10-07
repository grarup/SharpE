using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpE.Definitions.Collection;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.MvvmTools.Properties;

namespace SharpE.BaseEditors.Image
{
  class ImageViewerViewModel : IEditor
  {
    private UIElement m_view;
    private IFileViewModel m_file;
    private readonly IEnumerable<string> m_supportedFiles = new List<string>{".png"};
    private System.Windows.Controls.Image m_image;
    private readonly Dictionary<IFileViewModel, double> m_zooms = new Dictionary<IFileViewModel, double>();
    private readonly IObservableCollection<IMenuItemViewModel> m_menuItems = null;

    public string Name
    {
      get { return "ImageViewerViewModel"; }
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
          m_view = new ImageViewerView{DataContext = this};
          if (m_file != null)
            UpdateImage();
        }
        return m_view;
      }
    }

    private void UpdateImage()
    {
      BitmapImage bitmap = new BitmapImage();
      bitmap.BeginInit();
      bitmap.UriSource = new Uri(m_file.Path);
      bitmap.EndInit();
      m_image.Source = bitmap;
    }

    public IFileViewModel File
    {
      get { return m_file; }
      set
      {
        if (Equals(value, m_file)) return;
        m_file = value;
        if (m_file != null)
        {
          if (!m_zooms.ContainsKey(m_file))
            m_zooms.Add(m_file, 1);
          OnPropertyChanged("Zoom");
          if (m_view != null)
            UpdateImage();
        }
        OnPropertyChanged();
      }
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
      get { return m_zooms[m_file]; }
      set
      {
        if (value.Equals(m_zooms[m_file])) return;
        m_zooms[m_file] = value;
        OnPropertyChanged();
      }
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
