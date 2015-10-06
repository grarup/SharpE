using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using SharpE.Definitions.Editor;
using SharpE.Definitions.Project;
using SharpE.QubicleViewer.Properties;

namespace SharpE.QubicleViewer
{
  public class QubicleEditor : IEditor
  {
    #region declerations
    private readonly IFileViewModel m_settings = null;
    private UIElement m_view;
    private IFileViewModel m_file;
    private readonly List<string> m_supportedFiles = new List<string> { ".qb" };
    private Vector3D m_lookDirection = new Vector3D(1, 0, 0);
    private double m_zoom = 200;
    private Point3D m_center = new Point3D(19, 0, 5);
    private Point3D m_cameraPos;
    private bool m_rotate;
    private Point m_lastPosition;
    private double m_angleZaxis = -0.2;
    private double m_angleXaxis = -0.15;
    private Model3DGroup m_model;
    private readonly Dictionary<IFileViewModel, QbModel> m_cashcedModels = new Dictionary<IFileViewModel, QbModel>();
    private QbModel m_loadingModelGroup;
    private Timer m_loadingTimer;
    private QbModel m_qbModel;
    private CancellationTokenSource m_cancellationTokenSource;
    private double m_opacity;

    #endregion

    #region constructor
    public QubicleEditor()
    {
      UpdateCamera();

    }
    #endregion

    #region public properties
    public string Name
    {
      get { return "Qubicle editor"; }
    }

    public IFileViewModel Settings
    {
      get { return m_settings; }
    }

    public UIElement View
    {
      get
      {
        if (m_view == null)
        {
          m_view = new QubicleView { DataContext = this };
          Task task = new Task(() =>
          {
            QbModel model = new QbModel(Resources.Loading);
            model.CalcMesh();
            model.Generate3DModel(m_view.Dispatcher);
            model.Zoom = 200;
            model.AngleXaxis = -0.15;
            model.AngleZaxis = -0.2;
            model.Center = new Point3D(19,0,5);

            m_loadingModelGroup = model;
          });
          task.Start();
        }
        return m_view;
      }
    }

    public IFileViewModel File
    {
      get { return m_file; }
      set
      {
        if (Equals(value, m_file)) return;
        m_file = value;
        if (m_file == null) return;
        bool loadDone = false;
        lock (m_cashcedModels)
        {
          if (m_cashcedModels.ContainsKey(m_file))
          {
            if ((System.IO.File.GetLastWriteTime(m_file.Path) - m_cashcedModels[m_file].LoadTime).TotalMilliseconds < 0)

            QbModel = m_cashcedModels[m_file];
            loadDone = true;
          }
        }
        if (!loadDone)
        {
          if (m_loadingModelGroup != null)
          {
            m_loadingTimer = new Timer(state =>
            {
              QbModel = m_loadingModelGroup;
            }, null, 200, Timeout.Infinite);
          }

          if (m_cancellationTokenSource != null)
            m_cancellationTokenSource.Cancel();
          CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
          m_cancellationTokenSource = cancellationTokenSource;
          byte[] data = m_file.GetContent<byte[]>();
          Task task = new Task(() =>
          {
            QbModel model = new QbModel(data);
            if (cancellationTokenSource.Token.IsCancellationRequested)
              return;

            if (model.Parts.Count == 0)
              return;
            model.CalcMesh();
            if (m_cancellationTokenSource.Token.IsCancellationRequested)
              return;
            model.Generate3DModel(m_view.Dispatcher);

            if (m_loadingTimer != null)
            {
              m_loadingTimer.Change(Timeout.Infinite, Timeout.Infinite);
              m_loadingTimer.Dispose();
              m_loadingTimer = null;
            }

            lock (m_cashcedModels)
            {
              if (m_cashcedModels.ContainsKey(m_file))
                m_cashcedModels[m_file] = model;
              else
                m_cashcedModels.Add(m_file, model);
            }
            if (m_cancellationTokenSource.Token.IsCancellationRequested)
              return;
            QbModel = model;
          });
          task.Start();
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
      return new QubicleEditor();
    }

    public Vector3D LookDirection
    {
      get { return m_lookDirection; }
      set
      {
        if (value.Equals(m_lookDirection)) return;
        m_lookDirection = value;
        OnPropertyChanged();
      }
    }

    public Point3D Center
    {
      get { return m_center; }
      set
      {
        if (value.Equals(m_center)) return;
        m_center = value;
        UpdateCamera();
        OnPropertyChanged();
      }
    }

    public Point3D CameraPos
    {
      get { return m_cameraPos; }
      set
      {
        if (value.Equals(m_cameraPos)) return;
        m_cameraPos = value;
        OnPropertyChanged();
      }
    }

    public Action<int> Scroll
    {
      get { return ScrollHandler; }
    }

    public Action<bool> MouseIsDown
    {
      get { return n => Rotate = n; }
    }

    public bool Rotate
    {
      get { return m_rotate; }
      set
      {
        if (value.Equals(m_rotate)) return;
        m_rotate = value;
        OnPropertyChanged();
      }
    }

    public Action<Point> Move
    {
      get { return MoveHandler; }
    }

    public Model3DGroup Model
    {
      get { return m_model; }
      set
      {
        if (Equals(value, m_model)) return;
        m_model = value;
        OnPropertyChanged();
      }
    }

    public QbModel QbModel
    {
      get { return m_qbModel; }
      set
      {
        if (Equals(value, m_qbModel)) return;
        if (m_qbModel != null)
        {
          m_qbModel.Zoom = m_zoom;
          m_qbModel.AngleXaxis = m_angleXaxis;
          m_qbModel.AngleZaxis = m_angleZaxis;
          m_qbModel.PropertyChanged -= QbModelOnPropertyChanged;
        }
        m_qbModel = value;
        if (m_qbModel != null)
        {
          Opacity = m_qbModel.Opacity;
          m_zoom = m_qbModel.Zoom;
          m_angleXaxis = m_qbModel.AngleXaxis;
          m_angleZaxis = m_qbModel.AngleZaxis;
          m_center = m_qbModel.Center;
          UpdateCamera();
          Model = m_qbModel.Model3DGroup;
          m_qbModel.PropertyChanged += QbModelOnPropertyChanged;
        }
        OnPropertyChanged();
      }
    }

    private void QbModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      switch (propertyChangedEventArgs.PropertyName)
      {
        case "Scale":
          m_center = m_qbModel.Center;
          UpdateCamera();
          break;
      }
    }

    public double Opacity
    {
      get { return m_opacity; }
      set
      {
        if (value.Equals(m_opacity)) return;
        m_opacity = value;
        m_qbModel.Opacity = m_opacity;
        OnPropertyChanged();
      }
    }

    #endregion

    #region private methods
    private void MoveHandler(Point position)
    {
      if (m_rotate)
      {
        double xdiff = m_lastPosition.X - position.X;
        double ydiff = m_lastPosition.Y - position.Y;
        m_angleZaxis -= (xdiff / 100) % Math.PI;
        m_angleXaxis += (ydiff / 100);
        if (m_angleXaxis > Math.PI / 2)
          m_angleXaxis = Math.PI / 2;
        if (m_angleXaxis < Math.PI / -2)
          m_angleXaxis = Math.PI / -2;

        UpdateCamera();
      }
      m_lastPosition = position;
    }

    private void UpdateCamera()
    {
      LookDirection = new Vector3D(Math.Sin(m_angleZaxis), Math.Cos(m_angleZaxis), Math.Tan(m_angleXaxis));
      double factor = m_zoom /
                      (Math.Sqrt(Math.Pow(m_lookDirection.X, 2) + Math.Pow(m_lookDirection.Y, 2) +
                                 Math.Pow(m_lookDirection.Z, 2)));
      CameraPos = new Point3D(m_center.X - factor * m_lookDirection.X, m_center.Y - factor * m_lookDirection.Y,
                              m_center.Z - factor * m_lookDirection.Z);
    }

    private void ScrollHandler(int scroll)
    {
      m_zoom += m_zoom/10*(scroll > 0 ? 1 : -1); //scroll / (20.0);
      if (m_zoom < 1)
        m_zoom = 1;
      UpdateCamera();
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
