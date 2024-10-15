using g3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Solid;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using Point = Tekla.Structures.Geometry3d.Point;
using Polygon = TriangleNet.Geometry.Polygon;

namespace ExportTekla2Gltf
{
  public class TeklaGeoInfo
  {
    private long pointCounter = 0;//点 递增
    private long faceCounter = 0;//面，递增

    private long edgeCounter = 0;

    private Dictionary<long, List<long>> mArEdgeLines; //边界

    public bool CalData()
    {
      bool bre = false;
      Model currentModel = new Model();
      if (currentModel.GetConnectionStatus())
      {
        ModelObjectEnumerator enumeratorFrame
          = currentModel.GetModelObjectSelector().GetAllObjects();

        while (enumeratorFrame.MoveNext())
        {
          try
          {
            if (enumeratorFrame.Current is Part)//线性构件
            {
              Part part = enumeratorFrame.Current as Part;
              bre = EntityPart(part);
            }
            //else if (enumeratorFrame.Current is BoltArray)//螺栓
            //{
            //  BoltArray part = enumeratorFrame.Current as BoltArray;
            //  bre = EntityControlLine(part);
            //}
            else
            {
              //查看未知类型 添加监视容易中断
              var jhjh = enumeratorFrame.Current;
              int lkkl = 0;
            }
          }
          catch (Exception ex)
          {
            //非几何体
            string str = ex.ToString();
          }
        }  //  end while
      }

      return bre;
    }


    private bool EntityPart(Part part)
    {
      bool bre = false;
      //Init();

      //if (!IsHole(part))
      {
        GetTrianglesData(part);

        //GetRGB(part);
      }

      bre = true;
      return bre;
    }

    private void GetTrianglesData(Tekla.Structures.Model.Object obj)
    {
      Solid solid = null;
      if (obj is Part part)
      {
        solid = part.GetSolid();
      }
      else if (obj is BoltArray bolt)
      {
        solid = bolt.GetSolid();
      }

      if (solid != null)
      {
        GetData(solid);
      }

    }

    private void GetData(Solid solid)
    {
      var currentGeometry = curMapBinaryData["BirdXuan"];
      if (solid != null)
      {
        FaceEnumerator faceEnumerator = solid.GetFaceEnumerator();
        while (faceEnumerator.MoveNext())
        {
          var index = currentGeometry.vertexBuffer.Count / 3;
          //遍历面
          Face face = faceEnumerator.Current as Face;
          Vector normal = face.Normal;

          normal.Normalize();

          List<Point> points = new List<Point>();
          if (face is Face)
          {
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            while (loopEnumerator.MoveNext())
            {
              if (loopEnumerator.Current is Loop loop)
              {
                points.Clear();

                VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();

                List<long> faceVertexIndices = new List<long>();

                while (vertexEnumerator.MoveNext())
                {
                  if (vertexEnumerator.Current is Point pt)
                  {
                    points.Add(pt);

                    {
                      long pointIndex = pointCounter++;
                      faceVertexIndices.Add(pointIndex);
                    }
                  }
                }
                ///
                try
                {
                  Dictionary<Vector2d, KeyValuePair<int, Point>> Dic2Dim3 = new Dictionary<Vector2d, KeyValuePair<int, Point>>();

                  List<Vector2d> lst2D = new List<Vector2d>();
                  Vector3d point3D0 = new Vector3d(points[0].X, points[0].Y, points[0].Z);
                  for (int i = 0; i < points.Count; i++)
                  {
                    // 定义一个三维点
                    Vector3d point3D = new Vector3d(points[i].X, points[i].Y, points[i].Z);
                    // 定义平面上的一点和法线向量
                    Vector3d planePoint = point3D0;
                    Vector3d planeNormal = new Vector3d(normal.X, normal.Y, normal.Z); // 平面为XY平面

                    // 将三维点投影到平面上
                    Vector2d point2D = ProjectPointToPlane(point3D, planePoint, planeNormal);
                    lst2D.Add(point2D);
                    Dic2Dim3[point2D] = new KeyValuePair<int, Point>(i, points[i]);
                  }

                  if (Dic2Dim3.Count == 1)
                  {
                    int kkk = 0;
                    continue;
                  }

                  // 使用 Triangle.NET 进行 Delaunay 三角剖分
                  var polygon = new Polygon();
                  for (int i = 0; i < lst2D.Count; i++)
                  {
                    polygon.Add(new Vertex(lst2D[i].x, lst2D[i].y));
                  }

                  var mesh = polygon.Triangulate();
                  var validTriangles = mesh.Triangles.Where(t => IsTriangleInside(t, polygon));
                  Vertex v1, v2, v3;

                  foreach (var tri in validTriangles)
                  //foreach (var tri in mesh.Triangles)
                  {
                    v1 = tri.GetVertex(0);
                    v2 = tri.GetVertex(1);
                    v3 = tri.GetVertex(2);

                    {
                      currentGeometry.indexBuffer.Add(v1.ID + index);
                      currentGeometry.indexBuffer.Add(v2.ID + index);
                      currentGeometry.indexBuffer.Add(v3.ID + index);
                    }
                  }

                  //
                  foreach (var pt in points)
                  {
                    currentGeometry.vertexBuffer.Add((float)pt.X);
                    currentGeometry.vertexBuffer.Add((float)pt.Y);
                    currentGeometry.vertexBuffer.Add((float)pt.Z);

                    currentGeometry.normalBuffer.Add((float)normal.X);
                    currentGeometry.normalBuffer.Add((float)normal.Y);
                    currentGeometry.normalBuffer.Add((float)normal.Z);

                  }
                }
                catch (Exception ex)
                {
                  string str = ex.ToString();
                }

              }
            }////end while loop
          }
        }//end while face
      }
    }

    public Vector2d ProjectPointToPlane(Vector3d point, Vector3d planePoint, Vector3d planeNormal)
    {
      // 计算点到平面的投影
      Vector3d pointToPlane = point - planePoint;
      double distance = pointToPlane.Dot(planeNormal);
      Vector3d projectedPoint3D = point - distance * planeNormal;

      // 计算平面上的两个基向量
      Vector3d u = planeNormal.Cross(new Vector3d(1, 0, 0));
      if (u.LengthSquared == 0)
      {
        u = planeNormal.Cross(new Vector3d(0, 1, 0));
      }
      u.Normalize();

      Vector3d v = planeNormal.Cross(u);
      v.Normalize();

      // 将投影后的三维点转换为二维点
      Vector2d projectedPoint2D = new Vector2d(projectedPoint3D.Dot(u), projectedPoint3D.Dot(v));

      return projectedPoint2D;
    }



    public bool IsTriangleInside(ITriangle triangle, Polygon polygon)
    {
      // 获取三角形的三个顶点
      var p1 = triangle.GetVertex(0);
      var p2 = triangle.GetVertex(1);
      var p3 = triangle.GetVertex(2);

      // 检查三角形的中心点是否在多边形内部
      var centerX = (p1.X + p2.X + p3.X) / 3;
      var centerY = (p1.Y + p2.Y + p3.Y) / 3;

      if (!IsPointInPolygon(centerX, centerY, polygon))
      {
        return false;
      }

      // 检查三角形的边是否与多边形的边相交
      var triangleEdges = new[]
      {
        (p1, p2),
        (p2, p3),
        (p3, p1)
    };

      foreach (var edge in polygon.Segments)
      {
        foreach (var triangleEdge in triangleEdges)
        {
          if (DoLinesIntersect(triangleEdge.Item1, triangleEdge.Item2, edge.GetVertex(0), edge.GetVertex(1)))
          {
            return false;
          }
        }
      }

      return true;
    }

    private bool IsPointInPolygon(double x, double y, Polygon polygon)
    {
      bool inside = false;
      for (int i = 0, j = polygon.Points.Count - 1; i < polygon.Points.Count; j = i++)
      {
        var pi = polygon.Points[i];
        var pj = polygon.Points[j];
        if (((pi.Y > y) != (pj.Y > y)) &&
            (x < (pj.X - pi.X) * (y - pi.Y) / (pj.Y - pi.Y) + pi.X))
        {
          inside = !inside;
        }
      }
      return inside;
    }

    private bool DoLinesIntersect(Vertex a1, Vertex a2, Vertex b1, Vertex b2)
    {
      double det = (a2.X - a1.X) * (b2.Y - b1.Y) - (b2.X - b1.X) * (a2.Y - a1.Y);
      if (det == 0)
      {
        return false; // 线段平行
      }
      else
      {
        double lambda = ((b2.Y - b1.Y) * (b2.X - a1.X) + (b1.X - b2.X) * (b2.Y - a1.Y)) / det;
        double gamma = ((a1.Y - a2.Y) * (b2.X - a1.X) + (a2.X - a1.X) * (b2.Y - a1.Y)) / det;
        return (0 < lambda && lambda < 1) && (0 < gamma && gamma < 1);
      }
    }

    /// ////////////////////////////////////////////////////////////////////////////////////////////////////

    private GLTF glTF;

    private List<glTFBinaryData> allBinaryDatas;

    string gltfOutDir = "";
    private Dictionary<string, glTFBinaryData> curMapBinaryData = new Dictionary<string, glTFBinaryData>();
    public TeklaGeoInfo()
    {
      gltfOutDir = Path.GetDirectoryName(UIData.InstanceUI.m_strSavePath) + "\\";
      glTF = new GLTF();
      glTF.asset = new glTFVersion();
      glTF.scenes = new List<glTFScene>();
      glTF.nodes = new List<glTFNode>();
      glTF.meshes = new List<glTFMesh>();
      glTF.bufferViews = new List<glTFBufferView>();
      glTF.accessors = new List<glTFAccessor>();
      glTF.buffers = new List<glTFBuffer>();
      //glTF.materials = new List<glTFMaterial>();
      var scence = new glTFScene();
      scence.nodes = new List<int>() { 0 };
      glTF.scenes.Add(scence);
      glTFNode root = new glTFNode();
      root.name = "root";
      root.children = new List<int>();
      //设置y轴向上
      root.matrix = new List<double>()
            {
                0.3048, 0.0,0.0, 0.0,
                0.0,0.0, -0.3048, 0.0,
                0.0,0.3048,0.0,0.0,
                0.0,0.0,0.0, 1.0
            };
      glTF.nodes.Add(root);
      allBinaryDatas = new List<glTFBinaryData>();


      if (!curMapBinaryData.ContainsKey("BirdXuan"))
      {
        curMapBinaryData.Add("BirdXuan", new glTFBinaryData());
      }

    }

    public void PackageGeoData()
    {
      if (curMapBinaryData.Keys.Count > 0)
      {
        var node = new glTFNode();
        node.name = "BirdXuan";

        var meshID = glTF.meshes.Count;
        node.mesh = meshID;

        glTF.nodes.Add(node);

        glTF.nodes[0].children.Add(glTF.nodes.Count - 1);

        var mesh = new glTFMesh();
        glTF.meshes.Add(mesh);
        mesh.primitives = new List<glTFMeshPrimitive>();


        foreach (var key in curMapBinaryData.Keys)
        {
          var bufferData = curMapBinaryData[key];
          var primative = new glTFMeshPrimitive();
          //primative.material = MapMaterial[key].index;
          mesh.primitives.Add(primative);
          if (bufferData.indexBuffer.Count > 0)
          {
            glTFUtil.addIndexsBufferViewAndAccessor(glTF, bufferData);
            primative.indices = glTF.accessors.Count - 1;
          }
          if (bufferData.vertexBuffer.Count > 0)
          {
            glTFUtil.addVec3BufferViewAndAccessor(glTF, bufferData);
            primative.attributes.POSITION = glTF.accessors.Count - 1;
          }
          if (bufferData.normalBuffer.Count > 0)
          {
            glTFUtil.addNormalBufferViewAndAccessor(glTF, bufferData);
            primative.attributes.NORMAL = glTF.accessors.Count - 1;
          }
          if (bufferData.uvBuffer.Count > 0)
          {
            glTFUtil.addUvBufferViewAndAccessor(glTF, bufferData);
            primative.attributes.TEXCOORD_0 = glTF.accessors.Count - 1;
          }


          allBinaryDatas.Add(bufferData);
        }

        curMapBinaryData.Clear();
      }
    }

    public void WriteGltf()
    {
      MemoryStream memoryStream = new MemoryStream();
      using (BinaryWriter writer = new BinaryWriter(memoryStream))
      {

        foreach (var binData in allBinaryDatas)
        {
          foreach (var index in binData.indexBuffer)
          {
            if (binData.indexMax > 65535)
            {
              writer.Write((uint)index);
            }
            else
            {
              writer.Write((ushort)index);
            }
          }
          if (binData.indexAlign != null && binData.indexAlign != 0)
          {
            writer.Write((ushort)binData.indexAlign);
          }
          foreach (var coord in binData.vertexBuffer)
          {
            writer.Write((float)coord);
          }
          foreach (var normal in binData.normalBuffer)
          {
            writer.Write((float)normal);
          }
          foreach (var uv in binData.uvBuffer)
          {
            writer.Write((float)uv);
          }
        }
      }
      glTFBuffer newbuffer = new glTFBuffer();
      newbuffer.uri = Path.GetFileNameWithoutExtension(UIData.InstanceUI.m_strSavePath) + ".bin";
      newbuffer.byteLength = glTF.bufferViews[glTF.bufferViews.Count() - 1].byteOffset +
                   glTF.bufferViews[glTF.bufferViews.Count() - 1].byteLength;
      glTF.buffers = new List<glTFBuffer>() { newbuffer };

      //glTF.cameras = new List<glTFCameras>();

      var fileExtension = Path.GetExtension(UIData.InstanceUI.m_strSavePath).ToLower();
      if (fileExtension == ".gltf")
      {
        var binFileName = Path.GetFileNameWithoutExtension(UIData.InstanceUI.m_strSavePath) + ".bin";
        using (FileStream f = File.Create(Path.Combine(gltfOutDir, binFileName)))
        {
          byte[] data = memoryStream.ToArray();
          f.Write(data, 0, data.Length);
        }

        UTF8Encoding uTF8Encoding = new UTF8Encoding(false);
        File.WriteAllText(UIData.InstanceUI.m_strSavePath, glTF.toJson(), uTF8Encoding);
      }
    }

  }
}
