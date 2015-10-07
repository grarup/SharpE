using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using SharpE.MvvmTools.Properties;

namespace SharpE.QubicleViewer
{
  public class QbModel : INotifyPropertyChanged
  {
    private readonly List<QbModelPart> m_parts = new List<QbModelPart>();
    private Model3DGroup m_model3DGroup;
    private readonly List<Light> m_lights = new List<Light>();
    private double m_opacity = 1.0;
    private DateTime m_loadTime;
    private double m_zoom = 200;
    private Point3D m_center = new Point3D(19, 0, 5);
    private double m_angleZaxis = 0.75;
    private double m_angleXaxis = -0.5;
    private double m_scale = 0.1;
    private Ground m_ground;

    public QbModel(byte[] array)
    {
      Load(array);
    }

    public void Load(byte[] array)
    {
      if (array == null)
        return;
      m_loadTime = DateTime.Now;
      const int codeflag = 2;
      const int nextsliceflag = 6;

      BinaryReader binaryReader = new BinaryReader(new MemoryStream(array));

#pragma warning disable 168
      uint version = binaryReader.ReadUInt32();
      uint colorFormat = binaryReader.ReadUInt32();
      uint zAxisOrientation = binaryReader.ReadUInt32();
      uint compressed = binaryReader.ReadUInt32();
      uint visibilityMaskEncoded = binaryReader.ReadUInt32();
      uint numMatrices = binaryReader.ReadUInt32();

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
        QbModelPart matrix = new QbModelPart(sizeX, sizeY, sizeZ, posX, posY, posZ, name, Scale);
        m_parts.Add(matrix);

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
                  x = index % sizeX; // mod = modulo e.g. 12 mod 8 = 4
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
    }

    public List<QbModelPart> Parts
    {
      get { return m_parts; }
    }

    public double Opacity
    {
      get { return m_opacity; }
      set
      {
        if (value.Equals(m_opacity)) return;
        m_opacity = value;
        foreach (QbModelPart qbModelPart in m_parts)
        {
          qbModelPart.Opacity = m_opacity;
        }
      }
    }

    public Model3DGroup Model3DGroup
    {
      get { return m_model3DGroup; }
    }

    public DateTime LoadTime
    {
      get { return m_loadTime; }
    }

    public double Zoom
    {
      get { return m_zoom; }
      set
      {
        if (value.Equals(m_zoom)) return;
        m_zoom = value;
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
        OnPropertyChanged();
      }
    }

    public double AngleZaxis
    {
      get { return m_angleZaxis; }
      set { m_angleZaxis = value; }
    }

    public double AngleXaxis
    {
      get { return m_angleXaxis; }
      set { m_angleXaxis = value; }
    }

    public double Scale
    {
      get { return m_scale; }
      set
      {
        if (m_scale == value) return;
        m_scale = value;
        foreach (QbModelPart qbModelPart in m_parts)
        {
          qbModelPart.Scale = m_scale;
        }
        m_ground.Scale = m_scale;
        Center = DeterminCenter();
        OnPropertyChanged();
      }
    }

    public void CalcMesh()
    {
      foreach (QbModelPart qbModelPart in m_parts)
      {
        qbModelPart.CalcMesh();
      }
      Zoom = GuesZoom();
      Center = DeterminCenter();
    }

    public void Generate3DModel(Dispatcher dispatcher)
    {
      if (!dispatcher.CheckAccess())
      {
        dispatcher.Invoke(() => Generate3DModel(dispatcher));
        return ;
      }

      if (m_lights.Count == 0)
      {
        m_lights.Add(new DirectionalLight(Color.FromRgb(0xFF, 0xFF, 0xFF), new Vector3D(4, 6, -5)));
        m_lights.Add(new DirectionalLight(Color.FromRgb(0xFF, 0xFF, 0xFF), new Vector3D(-4, -6, 5)));
        m_lights.Add(new AmbientLight(Color.FromRgb(0x6F, 0x5F, 0x5F)));        
      }


      m_model3DGroup = new Model3DGroup();
      foreach (Light light in m_lights)
        m_model3DGroup.Children.Add(light);
      foreach (QbModelPart qbModelPart in m_parts)
      {
        qbModelPart.GenerateGeometry();
        foreach (GeometryModel3D geometryModel3D in qbModelPart.Geometry)
        {
          m_model3DGroup.Children.Add(geometryModel3D);
        }
      }

      //double maxX = m_parts.Max(n => n.Max.X);
      //double maxY = m_parts.Max(n => n.Max.Y);
      //double maxZ = m_parts.Max(n => n.Max.Z);
      //double minX = m_parts.Min(n => n.Min.X);
      //double minY = m_parts.Min(n => n.Min.Y);
      //double minZ = m_parts.Min(n => n.Min.Z);
      //BoundaryBox boundaryBox = new BoundaryBox(new Point3D(minX-0.5, minY-0.5, minZ-0.5), new Point3D(maxX + 0.5, maxY + 0.5, maxZ + 0.5), Colors.Purple);
      //m_model3DGroup.Children.Add(boundaryBox.GeometryModel3D);

      //double width = m_parts.Max(n => n.Width);
      //double height = m_parts.Max(n => n.Height);
      //double depth = m_parts.Max(n => n.Depth);
      //boundaryBox = new BoundaryBox(new Point3D(-0.5, -0.5, -0.5), new Point3D(width - 0.5, depth - 0.5, height - 0.5), Colors.Goldenrod);
      //m_model3DGroup.Children.Add(boundaryBox.GeometryModel3D);

      double maxX = m_parts.Max(n => n.Max.X) + 1;
      double maxY = m_parts.Max(n => n.Max.Y) + 1;
      double minX = m_parts.Min(n => n.Min.X);
      double minY = m_parts.Min(n => n.Min.Y);
      m_ground = new Ground((int) minX, (int) maxX, (int) minY, (int) maxY, Scale);
      foreach (GeometryModel3D geometryModel3D in m_ground.GeometryModel3D)
      {
        m_model3DGroup.Children.Add(geometryModel3D);
      }
    }

    public double GuesZoom()
    {
      double max = m_parts.Max(n => Math.Max(n.Width, n.Height));
      return max * 3 * m_scale;
    }

    public Point3D DeterminCenter()
    {
      double maxX = m_parts.Max(n => n.Max.X);
      double maxY = m_parts.Max(n => n.Max.Y);
      double maxZ = m_parts.Max(n => n.Max.Z);
      double minX = m_parts.Min(n => n.Min.X);
      double minY = m_parts.Min(n => n.Min.Y);
      double minZ = m_parts.Min(n => n.Min.Z);
      return new Point3D((minX + ((maxX - minX) / 2)) * m_scale, (minY + ((maxY - minY) / 2)) * m_scale, (minZ + ((maxZ - minZ) / 2)) * m_scale);
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
