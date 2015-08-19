using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
    private double m_angleXaxis= -0.15;
    private Model3DGroup m_model;
    private readonly Dictionary<IFileViewModel, Model3DGroup> m_cashcedModels = new Dictionary<IFileViewModel, Model3DGroup>();
    private Model3DGroup m_loadingModelGroup;
    private Timer m_loadingTimer;
    private readonly List<Light> m_lights = new List<Light>();
    #endregion

    #region constructor
    public QubicleEditor()
    {
      UpdateCamera();
      m_lights.Add(new DirectionalLight(Color.FromRgb(0xFF, 0xFF, 0xFF), new Vector3D(4, 6, -5)));
      m_lights.Add(new DirectionalLight(Color.FromRgb(0xFF, 0xFF, 0xFF), new Vector3D(-4, -6, 5)));
      m_lights.Add(new AmbientLight(Color.FromRgb(0x6F, 0x5F, 0x5F)));
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
          m_view = new QubicleView {DataContext = this};
          Task task = new Task(() =>
          {
            List<Model> models = LoadQubicleBinary(new MemoryStream(Resources.Loading));
            m_loadingModelGroup = CalcMesh(models);
            if (Model == null)
              Model = m_loadingModelGroup;
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
        if (m_file != null)
        {
          m_file.SetTag("center", m_center);
          m_file.SetTag("zoom", m_zoom);
          m_file.SetTag("angleZ", m_angleZaxis);
          m_file.SetTag("angleX", m_angleXaxis);
        }
        m_file = value;
        if (m_cashcedModels.ContainsKey(m_file))
        {
          m_zoom = (double) m_file.GetTag("zoom");
          m_angleZaxis = (double) m_file.GetTag("angleZ");
          m_angleXaxis = (double) m_file.GetTag("angleX");
          Center = (Point3D) m_file.GetTag("center");
          Model = m_cashcedModels[m_file];
        }
        else
        {
          m_loadingTimer = new Timer(state =>
          {
            m_zoom = 200;
            m_angleZaxis = -0.2;
            m_angleXaxis = -0.15;
            Center = new Point3D(19, 0, 5);
            Model = m_loadingModelGroup;
          }, null, 200, Timeout.Infinite);
        }
        Task task = new Task(() =>
        {
          List<Model> models = LoadQubicleBinary(m_file.GetContent<Stream>());
          Model3DGroup model3DGroup = CalcMesh(models);

          if (m_cashcedModels.ContainsKey(m_file))
            m_cashcedModels[m_file] = model3DGroup;
          else
          {
            m_cashcedModels.Add(m_file, model3DGroup);
            m_zoom = GuesZoom(models);
            m_angleZaxis = 0.75;
            m_angleXaxis = -0.5;
            Center = DeterminCenter(models);
          }

          Model = model3DGroup;
          if (m_loadingTimer != null)
          {
            m_loadingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            m_loadingTimer.Dispose();
            m_loadingTimer = null;
          }
        });
        task.Start();
        OnPropertyChanged();
      }
    }

    public IEnumerable<string> SupportedFiles
    {
      get { return m_supportedFiles; }
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
      m_zoom += scroll / 20.0;
      UpdateCamera();
    }

    private Model3DGroup CalcMesh(IEnumerable<Model> models)
    {
      if (models == null)
        return null;
      Dictionary<Color, List<Point3D>> points = new Dictionary<Color, List<Point3D>>();
      Dictionary<Color, List<int>> triangles = new Dictionary<Color, List<int>>();
      Dictionary<Color, List<Vector3D>> normals = new Dictionary<Color, List<Vector3D>>();
      foreach (Model model in models)
      {
        Point3D max = new Point3D(0, 0, 0);
        Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
        int index = 0;
        foreach (Qube qube in model.Qubes)
        {
          if (!points.ContainsKey(qube.Color))
            points.Add(qube.Color, new List<Point3D>());
          List<Point3D> point3DCollection = points[qube.Color];
          if (!triangles.ContainsKey(qube.Color))
            triangles.Add(qube.Color, new List<int>());
          List<int> int32Collection = triangles[qube.Color];
          if (!normals.ContainsKey(qube.Color))
            normals.Add(qube.Color, new List<Vector3D>());
          List<Vector3D> vector3DCollection = normals[qube.Color];

          Point3D point = model.GetPosition(index);
          if (qube.Mask != 0)
          {
            if (max.X < point.X)
              max.X = point.X;
            if (max.Y < point.Y)
              max.Y = point.Y;
            if (max.Z < point.Z)
              max.Z = point.Z;
            if (min.X > point.X)
              min.X = point.X;
            if (min.Y > point.Y)
                min.Y = point.Y;
            if (min.Z > point.Z)
              min.Z = point.Z;
            if (qube.Mask.Contains(QubeSideMask.Bottom))
            {
              Vector3D normal = new Vector3D(0, 0, 1);
              int index1 = Add(Offset(point, -0.5, -0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int index2 = Add(Offset(point, -0.5, 0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int index3 = Add(Offset(point, 0.5, 0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int index4 = Add(Offset(point, 0.5, -0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int32Collection.Add(index1);
              int32Collection.Add(index2);
              int32Collection.Add(index3);
              int32Collection.Add(index1);
              int32Collection.Add(index3);
              int32Collection.Add(index4);
            }
            if (qube.Mask.Contains(QubeSideMask.Top))
            {
              Vector3D normal = new Vector3D(0, 0, 1);
              int index1 = Add(Offset(point, -0.5, -0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index2 = Add(Offset(point, -0.5, 0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index3 = Add(Offset(point, 0.5, 0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index4 = Add(Offset(point, 0.5, -0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int32Collection.Add(index1);
              int32Collection.Add(index3);
              int32Collection.Add(index2);
              int32Collection.Add(index1);
              int32Collection.Add(index4);
              int32Collection.Add(index3);
            }
            if (qube.Mask.Contains(QubeSideMask.Right))
            {
              Vector3D normal = new Vector3D(-1, 0, 0);
              int index1 = Add(Offset(point, -0.5, -0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int index2 = Add(Offset(point, -0.5, -0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index3 = Add(Offset(point, -0.5, 0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index4 = Add(Offset(point, -0.5, 0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int32Collection.Add(index1);
              int32Collection.Add(index2);
              int32Collection.Add(index3);
              int32Collection.Add(index1);
              int32Collection.Add(index3);
              int32Collection.Add(index4);
            }
            if (qube.Mask.Contains(QubeSideMask.Left))
            {
              Vector3D normal = new Vector3D(1, 0, 0);
              int index1 = Add(Offset(point, 0.5, -0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int index2 = Add(Offset(point, 0.5, -0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index3 = Add(Offset(point, 0.5, 0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index4 = Add(Offset(point, 0.5, 0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int32Collection.Add(index1);
              int32Collection.Add(index3);
              int32Collection.Add(index2);
              int32Collection.Add(index1);
              int32Collection.Add(index4);
              int32Collection.Add(index3);
            }
            if (qube.Mask.Contains(QubeSideMask.Back))
            {
              Vector3D normal = new Vector3D(0, 1, 0);
              int index1 = Add(Offset(point, -0.5, 0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int index2 = Add(Offset(point, -0.5, 0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index3 = Add(Offset(point, 0.5, 0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index4 = Add(Offset(point, 0.5, 0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int32Collection.Add(index1);
              int32Collection.Add(index2);
              int32Collection.Add(index3);
              int32Collection.Add(index1);
              int32Collection.Add(index3);
              int32Collection.Add(index4);
            }
            if (qube.Mask.Contains(QubeSideMask.Front))
            {
              Vector3D normal = new Vector3D(0, -1, 0);
              int index1 = Add(Offset(point, -0.5, -0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int index2 = Add(Offset(point, -0.5, -0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index3 = Add(Offset(point, 0.5, -0.5, 0.5), normal, point3DCollection, vector3DCollection);
              int index4 = Add(Offset(point, 0.5, -0.5, -0.5), normal, point3DCollection, vector3DCollection);
              int32Collection.Add(index1);
              int32Collection.Add(index3);
              int32Collection.Add(index2);
              int32Collection.Add(index1);
              int32Collection.Add(index4);
              int32Collection.Add(index3);
            }
          }
          index++;
        }
        model.Min = min;
        model.Max = max;
      }
      return GenerateMesh(points, triangles, normals);
    }

    private Model3DGroup GenerateMesh(Dictionary<Color, List<Point3D>> points, Dictionary<Color, List<int>> triangles, Dictionary<Color, List<Vector3D>> normals)
    {
      if (!m_view.Dispatcher.CheckAccess())
      {
        Model3DGroup retVal = null;
        m_view.Dispatcher.Invoke(() => retVal = GenerateMesh(points, triangles, normals));
        return retVal;
      }

      Model3DGroup model3DGroup = new Model3DGroup();
      foreach (Light light in m_lights)
        model3DGroup.Children.Add(light);
      foreach (Color color in points.Keys)
      {
        if (points[color].Count == 0)
          continue;
        GeometryModel3D geometryModel3D = new GeometryModel3D();
        MeshGeometry3D meshGeometry3D = new MeshGeometry3D
          {
            Positions = new Point3DCollection(points[color]),
            TriangleIndices = new Int32Collection(triangles[color]),
            Normals = new Vector3DCollection(normals[color])
          };
        geometryModel3D.Geometry = meshGeometry3D;
        geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(color));
        model3DGroup.Children.Add(geometryModel3D);
      }

      return model3DGroup;
    }

    private static double GuesZoom(ICollection<Model> models)
    {
      double max = models == null || models.Count == 0  ? 10 : models.Max(n => Math.Max(n.Width, n.Height));
      return max * 3;
    }

    private Point3D DeterminCenter(List<Model> models)
    {
      if (models == null)
        return new Point3D();
      double maxX = models.Max(n => n.Max.X);
      double maxY = models.Max(n => n.Max.Y);
      double maxZ = models.Max(n => n.Max.Z);
      double minX = models.Min(n => n.Min.X);
      double minY = models.Min(n => n.Min.Y);
      double minZ = models.Min(n => n.Min.Z);
      return new Point3D(minX + ((maxX - minX)/2), minY + ((maxY - minY)/2),minZ + ((maxZ - minZ)/2));
    }

    private Point3D Offset(Point3D point, double x, double y, double z)
    {
      return new Point3D(point.X + x, point.Y + y, point.Z + z);
    }

    private int Add(Point3D point3D, Vector3D normal, List<Point3D> point3DCollection, List<Vector3D> normals)
    {
      int index = point3DCollection.IndexOf(point3D);
      if (index == -1 || normals[index] != normal)
      {
        index = point3DCollection.Count;
        point3DCollection.Add(point3D);
        normals.Add(normal);
      }
      return index;
    }

    public List<Model> LoadQubicleBinary(Stream stream)
    {
      if (stream == null)
        return null;
      const int codeflag = 2;
      const int nextsliceflag = 6;

      BinaryReader binaryReader = new BinaryReader(stream);

#pragma warning disable 168
      uint version = binaryReader.ReadUInt32();
      uint colorFormat = binaryReader.ReadUInt32();
      uint zAxisOrientation = binaryReader.ReadUInt32();
      uint compressed = binaryReader.ReadUInt32();
      uint visibilityMaskEncoded = binaryReader.ReadUInt32();
      uint numMatrices = binaryReader.ReadUInt32();
      List<Model> matrixList = new List<Model>();

      for (int i = 0; i < numMatrices; i++) // for each matrix stored in file
      {
        // read matrix name
        byte nameLength = binaryReader.ReadByte();
        string name = new string(binaryReader.ReadChars(nameLength));
#pragma warning restore 168

        // read matrix size 
        uint sizeX = binaryReader.ReadUInt32();
        uint sizeY = binaryReader.ReadUInt32();
        uint sizeZ = binaryReader.ReadUInt32();

        // read matrix position (in this example the position is irrelevant)
        int posX = binaryReader.ReadInt32();
        int posY = binaryReader.ReadInt32();
        int posZ = binaryReader.ReadInt32();

        // create matrix and add to matrix list
        Model matrix = new Model(sizeX, sizeY, sizeZ, posX, posY, posZ);
        matrixList.Add(matrix);

        uint x, y, z;
        if (compressed == 0) // if uncompressd
        {
          for (z = 0; z < sizeZ; z++)
            for (y = 0; y < sizeY; y++)
              for (x = 0; x < sizeX; x++)
                matrix.Qubes[x + y * sizeX + z * sizeX * sizeY] = new Qube(binaryReader.ReadUInt32(), colorFormat);
        }
        else // if compressed
        {
          z = 0;

          while (z < sizeZ)
          {
            uint index = 0;

            while (true)
            {
              uint data = binaryReader.ReadUInt32();

              if (data == nextsliceflag)
                break;
              if (data == codeflag)
              {
                uint count = binaryReader.ReadUInt32();
                data = binaryReader.ReadUInt32();

                for (uint j = 0; j < count; j++)
                {
                  x = index % sizeX ; // mod = modulo e.g. 12 mod 8 = 4
                  y = index / sizeX; // div = integer division e.g. 12 div 8 = 1
                  index++;
                  matrix.Qubes[x + y * sizeX + z * sizeX * sizeY] = new Qube(data, colorFormat);
                }
              }
              else
              {
                x = index % sizeX;
                y = index / sizeX;
                index++;
                matrix.Qubes[x + y * sizeX + z * sizeX * sizeY] = new Qube(data, colorFormat);
              }
            }
            z++;
          }
        }
      }
      return matrixList;
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
