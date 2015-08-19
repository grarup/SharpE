using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SharpE.QubicleViewer
{
  class BoundaryBox
  {
    private Point3D m_min;
    private Point3D m_max;
    private readonly Color m_color;
    private GeometryModel3D m_geometryModel3D;

    public BoundaryBox(Point3D mMin, Point3D mMax, Color color)
    {
      m_min = mMin;
      m_max = mMax;
      m_color = color;
      CreateMesh();
    }

    public GeometryModel3D GeometryModel3D
    {
      get { return m_geometryModel3D; }
    }

    public void CreateMesh()
    {
      Point3DCollection point3DCollection = new Point3DCollection();
      //Front
      point3DCollection.Add(new Point3D(m_min.X, m_min.Y, m_min.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_min.Y, m_min.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_max.Y, m_min.Z));
      point3DCollection.Add(new Point3D(m_min.X, m_max.Y, m_min.Z));
      //Back
      point3DCollection.Add(new Point3D(m_min.X, m_min.Y, m_max.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_min.Y, m_max.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_max.Y, m_max.Z));
      point3DCollection.Add(new Point3D(m_min.X, m_max.Y, m_max.Z));
      //Left 
      point3DCollection.Add(new Point3D(m_min.X, m_min.Y, m_max.Z));
      point3DCollection.Add(new Point3D(m_min.X, m_min.Y, m_min.Z));
      point3DCollection.Add(new Point3D(m_min.X, m_max.Y, m_min.Z));
      point3DCollection.Add(new Point3D(m_min.X, m_max.Y, m_max.Z));
      //Right
      point3DCollection.Add(new Point3D(m_max.X, m_min.Y, m_max.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_min.Y, m_min.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_max.Y, m_min.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_max.Y, m_max.Z));
      //Bottom
      point3DCollection.Add(new Point3D(m_min.X, m_min.Y, m_max.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_min.Y, m_max.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_min.Y, m_min.Z));
      point3DCollection.Add(new Point3D(m_min.X, m_min.Y, m_min.Z));
      //Top
      point3DCollection.Add(new Point3D(m_min.X, m_max.Y, m_max.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_max.Y, m_max.Z));
      point3DCollection.Add(new Point3D(m_max.X, m_max.Y, m_min.Z));
      point3DCollection.Add(new Point3D(m_min.X, m_max.Y, m_min.Z));

      Vector3DCollection normals = new Vector3DCollection();
      normals.Add(new Vector3D(0, 0, 1));
      normals.Add(new Vector3D(0, 0, 1));
      normals.Add(new Vector3D(0, 0, 1));
      normals.Add(new Vector3D(0, 0, 1));

      normals.Add(new Vector3D(0, 0, -1));
      normals.Add(new Vector3D(0, 0, -1));
      normals.Add(new Vector3D(0, 0, -1));
      normals.Add(new Vector3D(0, 0, -1));

      normals.Add(new Vector3D(-1, 0, 0));
      normals.Add(new Vector3D(-1, 0, 0));
      normals.Add(new Vector3D(-1, 0, 0));
      normals.Add(new Vector3D(-1, 0, 0));

      normals.Add(new Vector3D(1, 0, 0));
      normals.Add(new Vector3D(1, 0, 0));
      normals.Add(new Vector3D(1, 0, 0));
      normals.Add(new Vector3D(1, 0, 0));

      normals.Add(new Vector3D(0, -1, 0));
      normals.Add(new Vector3D(0, -1, 0));
      normals.Add(new Vector3D(0, -1, 0));
      normals.Add(new Vector3D(0, -1, 0));

      normals.Add(new Vector3D(0, 1, 0));
      normals.Add(new Vector3D(0, 1, 0));
      normals.Add(new Vector3D(0, 1, 0));
      normals.Add(new Vector3D(0, 1, 0));

      Int32Collection triangles = new Int32Collection();

      //for (int i = 1; i < 6; i += 2)
      //{
      //  triangles.Add(0 + (i * 4));
      //  triangles.Add(1 + (i * 4));
      //  triangles.Add(3 + (i * 4));
      //  triangles.Add(1 + (i * 4));
      //  triangles.Add(2 + (i * 4));
      //  triangles.Add(3 + (i * 4));
      //}

      //for (int i = 0; i < 6; i += 2)
      //{
      //  triangles.Add(3 + (i * 4));
      //  triangles.Add(1 + (i * 4));
      //  triangles.Add(0 + (i * 4));
      //  triangles.Add(3 + (i * 4));
      //  triangles.Add(2 + (i * 4));
      //  triangles.Add(1 + (i * 4));
      //}

      for (int i = 0; i < 6; i++)
      {
        triangles.Add(0 + (i * 4));
        triangles.Add(1 + (i * 4));
        triangles.Add(0 + (i * 4));
        triangles.Add(1 + (i * 4));
        triangles.Add(2 + (i * 4));
        triangles.Add(1 + (i * 4));
        triangles.Add(2 + (i * 4));
        triangles.Add(3 + (i * 4));
        triangles.Add(2 + (i * 4));
        triangles.Add(3 + (i * 4));
        triangles.Add(0 + (i * 4));
        triangles.Add(3 + (i * 4));
      }


      MeshGeometry3D meshGeometry3D = new MeshGeometry3D
        {
          Positions = point3DCollection,
          Normals = normals,
          TriangleIndices = triangles
        };

      m_geometryModel3D = new GeometryModel3D();
      GeometryModel3D.Geometry = meshGeometry3D.ToWireframe(0.1);
      GeometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(m_color));
    }
  }
}
