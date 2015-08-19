using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using SharpE.MvvmTools.Properties;

namespace SharpE.QubicleViewer
{
  public class QbModelPart : INotifyPropertyChanged
  {
    private readonly Qube[] m_qubes;
    private readonly uint m_width;
    private readonly uint m_height;
    private readonly uint m_depth;
    private readonly int m_x;
    private readonly int m_y;
    private readonly int m_z;
    private readonly string m_name;
    private Point3D m_min;
    private Point3D m_max;
    private Dictionary<Color, List<Point3D>> m_points;
    private Dictionary<Color, List<int>> m_triangles;
    private Dictionary<Color, List<Vector3D>> m_normals;
    private List<GeometryModel3D> m_geometry;
    private List<DiffuseMaterial> m_materials;
    private bool m_isVisable = true;
    private double m_opacity = 1.0;
    private EmissiveMaterial m_hideMaterial;

    public QbModelPart(uint width, uint height, uint depth, int x, int y, int z, string name)
    {
      m_width = width;
      m_height = height;
      m_depth = depth;
      m_x = x;
      m_y = y;
      m_z = z;
      m_name = name ?? "<no name>";
      m_qubes = new Qube[width * height * depth];
    }

    public void CalcMesh()
    {
      m_points = new Dictionary<Color, List<Point3D>>();
      m_triangles = new Dictionary<Color, List<int>>();
      m_normals = new Dictionary<Color, List<Vector3D>>();
      Point3D max = new Point3D(0, 0, 0);
      Point3D min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
      int index = 0;
      foreach (Qube qube in m_qubes)
      {
        if (!m_points.ContainsKey(qube.Color))
          m_points.Add(qube.Color, new List<Point3D>());
        List<Point3D> point3DCollection = m_points[qube.Color];
        if (!m_triangles.ContainsKey(qube.Color))
          m_triangles.Add(qube.Color, new List<int>());
        List<int> int32Collection = m_triangles[qube.Color];
        if (!m_normals.ContainsKey(qube.Color))
          m_normals.Add(qube.Color, new List<Vector3D>());
        List<Vector3D> vector3DCollection = m_normals[qube.Color];

        Point3D point = GetPosition(index);
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
        Min = min;
        Max = max;
      }

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

    public Point3D GetPosition(int index)
    {
      int a = (int)(index / (m_width * m_height));
      int z = (int)((index - (a * m_width * m_height)) / m_width);
      int x = (int)(index - (a * m_width * m_height) - (z * m_width));
      return new Point3D(x, a, z);
    }

    public void GenerateGeometry()
    {
      m_hideMaterial = new EmissiveMaterial(new SolidColorBrush(Colors.Transparent) { Opacity = 0 });
      m_geometry = new List<GeometryModel3D>();
      m_materials = new List<DiffuseMaterial>();
      foreach (Color color in m_points.Keys)
      {
        if (m_points[color].Count == 0)
          continue;
        GeometryModel3D geometryModel3D = new GeometryModel3D();
        MeshGeometry3D meshGeometry3D = new MeshGeometry3D
        {
          Positions = new Point3DCollection(m_points[color]),
          TriangleIndices = new Int32Collection(m_triangles[color]),
          Normals = new Vector3DCollection(m_normals[color])
        };
        geometryModel3D.Geometry = meshGeometry3D;
        DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(color) {Opacity = m_isVisable ? Opacity : 0}) ;
        geometryModel3D.Material = material;
        m_materials.Add(material);

        Geometry.Add(geometryModel3D);
      }
      
    }

    public Point3D Center
    {
      get { return new Point3D(m_width / 2.0, m_height / 2.0, m_depth / 2.0); }
    }

    public Qube[] Qubes
    {
      get { return m_qubes; }
    }

    public uint Width
    {
      get { return m_width; }
    }

    public uint Height
    {
      get { return m_height; }
    }

    public uint Depth
    {
      get { return m_depth; }
    }

    public int X
    {
      get { return m_x; }
    }

    public int Y
    {
      get { return m_y; }
    }

    public int Z
    {
      get { return m_z; }
    }

    public Point3D Min
    {
      get { return m_min; }
      set
      {
        if (value.Equals(m_min)) return;
        m_min = value;
        OnPropertyChanged();
      }
    }

    public Point3D Max
    {
      get { return m_max; }
      set
      {
        if (value.Equals(m_max)) return;
        m_max = value;
        OnPropertyChanged();
      }
    }

    public string Name
    {
      get { return m_name; }
    }

    public List<GeometryModel3D> Geometry
    {
      get { return m_geometry; }
    }

    public bool IsVisable
    {
      get { return m_isVisable; }
      set
      {
        if (value.Equals(m_isVisable)) return;
        m_isVisable = value;
        
        for (int index = 0; index < m_geometry.Count; index++)
        {
          GeometryModel3D geometryModel3D = m_geometry[index];
          if (m_isVisable)
            geometryModel3D.Material = m_materials[index];
          else
            geometryModel3D.Material = m_hideMaterial;
        }
        OnPropertyChanged();
      }
    }

    private void UpdateOpacity()
    {
      foreach (DiffuseMaterial diffuseMaterial in m_materials)
      {
        diffuseMaterial.Brush.Opacity = m_opacity;
      }
    }

    public double Opacity
    {
      get { return m_opacity; }
      set
      {
        if (value.Equals(m_opacity)) return;
        m_opacity = value;
        UpdateOpacity();
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
