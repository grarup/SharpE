using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SharpE.QubicleViewer
{
  class Ground
  {
    private readonly int m_minX;
    private readonly int m_maxX;
    private readonly int m_minY;
    private readonly int m_maxY;
    private double m_scale;
    private readonly List<GeometryModel3D> m_geometryModel3D = new List<GeometryModel3D>();

    public Ground(int minX, int maxX, int minY, int maxY, double scale)
    {
      m_minX = minX;
      m_maxX = maxX;
      m_minY = minY;
      m_maxY = maxY;
      m_scale = scale;
      GeometryModel3D geometryModel3D = new GeometryModel3D();
      geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Green));
      m_geometryModel3D.Add(geometryModel3D);

      geometryModel3D = new GeometryModel3D();
      geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.DarkGreen));
      m_geometryModel3D.Add(geometryModel3D);

      geometryModel3D = new GeometryModel3D();
      geometryModel3D.Material = new DiffuseMaterial(new SolidColorBrush(Colors.RoyalBlue));
      m_geometryModel3D.Add(geometryModel3D);
      
      CreateMesh(minX, maxX, minY, maxY, scale);

    }

    private void CreateMesh(int minX, int maxX, int minY, int maxY, double scale)
    {
      List<Point3DCollection> pointCollections = new List<Point3DCollection>();
      List<Vector3DCollection> normals = new List<Vector3DCollection>();

      pointCollections.Add(new Point3DCollection());
      pointCollections.Add(new Point3DCollection());

      normals.Add(new Vector3DCollection());
      normals.Add(new Vector3DCollection());

      

      int flip = 0;
      int start = 0;
      for (int x = (int)(Math.Floor((minX - 1) * scale) ); x < Math.Ceiling(maxX * scale) + 1; x++)
      {
        for (int y = (int)(Math.Floor((minY  - 1) * scale) ); y < Math.Ceiling(maxY * scale) + 1; y++)
        {
          pointCollections[flip].Add(new Point3D((x - 0.5), (y - 0.5), -0.5 * scale));
          pointCollections[flip].Add(new Point3D((x + 1 - 0.5), (y - 0.5), -0.5 * scale));
          pointCollections[flip].Add(new Point3D((x + 1 - 0.5), (y + 1 - 0.5), -0.5 * scale));
          pointCollections[flip].Add(new Point3D((x - 0.5), (y + 1 - 0.5), -0.5 * scale));
          normals[flip].Add(new Vector3D(0, 0, -1));
          normals[flip].Add(new Vector3D(0, 0, -1));
          normals[flip].Add(new Vector3D(0, 0, -1));
          normals[flip].Add(new Vector3D(0, 0, -1));
          flip = flip == 0 ? 1 : 0;
        }
        flip = start == 0 ? 1 : 0;
        start = flip;
      }

      Int32Collection triangles = new Int32Collection();
      for (int i = 0; i < pointCollections[0].Count/4; i++)
      {
        triangles.Add(0 + (i*4));
        triangles.Add(1 + (i*4));
        triangles.Add(2 + (i*4));
        triangles.Add(0 + (i*4));
        triangles.Add(2 + (i*4));
        triangles.Add(3 + (i*4));
      }

      MeshGeometry3D meshGeometry3D = new MeshGeometry3D
        {
          Positions = pointCollections[0],
          Normals = normals[0],
          TriangleIndices = triangles
        };

      m_geometryModel3D[0].Geometry = meshGeometry3D;

      meshGeometry3D = new MeshGeometry3D
        {
          Positions = pointCollections[1],
          Normals = normals[1],
          TriangleIndices = triangles
        };

      m_geometryModel3D[1].Geometry = meshGeometry3D;
    }

    public List<GeometryModel3D> GeometryModel3D
    {
      get { return m_geometryModel3D; }
    }

    public double Scale
    {
      get { return m_scale; }
      set
      {
        if (m_scale == value) return;
        m_scale = value;
        CreateMesh(m_minX, m_maxX, m_minY, m_maxY, m_scale);
      }
    }
  }
}
