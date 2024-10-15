using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Solid;

using System;
using System.Collections.Generic;
using System.Numerics; // 用于 Matrix4x4
using Tekla.Structures.Geometry3d; // 用于 Point, Vector 等
using Tekla.Structures.Model; // 用于 Solid, Face 等
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using Point = Tekla.Structures.Geometry3d.Point;
using Polygon = TriangleNet.Geometry.Polygon;

namespace ExportTekla2Gltf
{
  public class PolymeshFacet
  {
    public PolymeshFacet(int v1, int v2, int v3)
    {
      V1 = v1;
      V2 = v2;
      V3 = v3;
      IsValidObject = true;
    }

    public bool IsValidObject { get; }
    public int V3 { get; }
    public int V2 { get; }
    public int V1 { get; }

    public IList<int> GetVertices() => new List<int> { V1, V2, V3 };

    public override string ToString() => $"Facet: {V1}, {V2}, {V3}";
  }


  public class TeklaGeoInfo
  {
    private Dictionary<long, Point> mArVertex3f; //点
    private Dictionary<long, Point> mArNormal3f; //定点的法向量
    private Dictionary<long, List<long>> mArEdgeLines; //边界
    private Dictionary<long, List<long>> mArFace; //面(广义面,包括点和线)

    private Dictionary<long, Point> mArVertex3fTemp; //点,临时
    private long mLArVertex3fTemp = 0;
    private int mnRgb = 0;//控制构件颜色

    private long pointCounter = 0;
    private long edgeCounter = 0;
    private long faceCounter = 0;
    private List<Point> points1PP = new List<Point>();

    public bool CalData()
    {
      Init();
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
        mArVertex3fTemp.Clear();
        mLArVertex3fTemp = 0;
        while (faceEnumerator.MoveNext())
        {
          //遍历面
          Face face = faceEnumerator.Current as Face;
          Vector normal = face.Normal;
          
          if (face is Face)
          {
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            while (loopEnumerator.MoveNext())
            {
              if (loopEnumerator.Current is Loop loop)
              {
                List<Point> points = new List<Point>();

                VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();

                points.Clear();

                while (vertexEnumerator.MoveNext())
                {
                  if (vertexEnumerator.Current is Point pt)
                  {
                    points.Add(pt);
                  }
                }
                if (points.Count > 2)
                {
                  List<long> faceVertexIndices = new List<long>();
                  foreach (var pt in points)
                  {
                    long pointIndex = pointCounter++;
                    mArVertex3f[pointIndex] = new Point(pt.X, pt.Y, pt.Z);
                    mArNormal3f[pointIndex] = new Point(normal.X, normal.Y, normal.Z);
                    faceVertexIndices.Add(pointIndex);

                  }

                  mArFace[faceCounter++] = faceVertexIndices;

                  if (!IsAllRepeat(points))
                  {
                   // Triangulate(points, faceVertexIndices);
                  }

                }
              }
            }////end while loop
          }
        }//end while face
      }
    }

    #region 剖分


private void GetDatanew(Solid solid)
  {
    glTFBinaryData currentGeometry = curMapBinaryData["BirdXuan"];
    if (solid != null)
    {
      FaceEnumerator faceEnumerator = solid.GetFaceEnumerator();
      mArVertex3fTemp.Clear();
      mLArVertex3fTemp = 0;
      int indexOffset = currentGeometry.vertexBuffer.Count / 3;
      while (faceEnumerator.MoveNext())
      {
        Face face = faceEnumerator.Current as Face;
        Vector normal = face.Normal;
        if (face is Face)
        {
          List<Point> facePoints = new List<Point>();
          LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
          while (loopEnumerator.MoveNext())
          {
            if (loopEnumerator.Current is Loop loop)
            {
              VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();
              while (vertexEnumerator.MoveNext())
              {
                Point point = vertexEnumerator.Current;
                facePoints.Add(point);
              }
            }
          }

          if (facePoints.Count >= 3)
          {
            // 创建一个平面变换以将3D点投影到2D平面
            Matrix4x4 transformMatrix = GetTransformToXYPlane(normal, facePoints[0]);

            // 创建Triangle.NET的多边形输入
            Polygon polygon = new Polygon();

            // 添加顶点到多边形
            for (int i = 0; i < facePoints.Count; i++)
            {
              System.Numerics.Vector3 transformedPoint = TransformPoint(facePoints[i], transformMatrix);
              polygon.Add(new Vertex(transformedPoint.X, transformedPoint.Y));

              // 添加顶点到vertexBuffer
              currentGeometry.vertexBuffer.Add((float)facePoints[i].X);
              currentGeometry.vertexBuffer.Add((float)facePoints[i].Y);
              currentGeometry.vertexBuffer.Add((float)facePoints[i].Z);

              // 添加法线到normalBuffer
              if (normal != null)
              {
                currentGeometry.normalBuffer.Add((float)normal.X);
                currentGeometry.normalBuffer.Add((float)normal.Y);
                currentGeometry.normalBuffer.Add((float)normal.Z);
              }
            }

            // 配置三角剖分选项
            var options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
            var quality = new TriangleNet.Meshing.QualityOptions() { MinimumAngle = 20 };

            // 执行三角剖分
            var mesh = (TriangleNet.Mesh)polygon.Triangulate(options, quality);

            // 添加三角形索引
            foreach (ITriangle triangle in mesh.Triangles)
            {
              currentGeometry.indexBuffer.Add(indexOffset + triangle.GetVertexID(0));
              currentGeometry.indexBuffer.Add(indexOffset + triangle.GetVertexID(1));
              currentGeometry.indexBuffer.Add(indexOffset + triangle.GetVertexID(2));
            }

            indexOffset += facePoints.Count;
          }
        }
      }
    }
  }

  // 辅助方法：创建从给定平面到XY平面的变换矩阵
  private Matrix4x4 GetTransformToXYPlane(Vector normal, Point origin)
  {
    Vector3 zAxis = new Vector3((float)normal.X, (float)normal.Y, (float)normal.Z);
    zAxis = Vector3.Normalize(zAxis);
    Vector3 xAxis = Vector3.Normalize(Vector3.Cross(new Vector3(0, 0, 1), zAxis));
    Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

    Matrix4x4 rotation = new Matrix4x4(
        xAxis.X, yAxis.X, zAxis.X, 0,
        xAxis.Y, yAxis.Y, zAxis.Y, 0,
        xAxis.Z, yAxis.Z, zAxis.Z, 0,
        0, 0, 0, 1
    );

    Matrix4x4 translation = Matrix4x4.CreateTranslation(-(float)origin.X, -(float)origin.Y, -(float)origin.Z);

    return Matrix4x4.Multiply(translation, rotation);
  }

  // 辅助方法：使用变换矩阵转换点
  private Vector3 TransformPoint(Point point, Matrix4x4 matrix)
  {
    Vector4 vector = Vector4.Transform(new Vector4((float)point.X, (float)point.Y, (float)point.Z, 1), matrix);
    return new Vector3(vector.X, vector.Y, vector.Z);
  }
  #endregion



  /// <summary>
  /// 是否重复，重复返回true
  /// </summary>
  /// <param name="lstpoint"></param>
  /// <returns></returns>
  private bool IsAllRepeat(List<Point> lstpoint)
    {
      bool bre = false;
      if (mArVertex3fTemp.Count == 0)
      {  
        foreach (var item in lstpoint)
        {
          mArVertex3fTemp[mLArVertex3fTemp++] = item;
        }
      }
      else
      {
        bre = lstpoint.All(point => mArVertex3fTemp.ContainsValue(point));
        if (!bre)
        {
          int i = 0;
          foreach (var item in lstpoint)
          {
            mArVertex3fTemp[mLArVertex3fTemp++] = item;
          }
        }
      }

      

      return bre;
    }


    private void Init()
    {
      mArVertex3f = new Dictionary<long, Point>();
      mArNormal3f = new Dictionary<long, Point>();
      mArEdgeLines = new Dictionary<long, List<long>>();
      mArFace = new Dictionary<long, List<long>>();
      mArVertex3fTemp = new Dictionary<long, Point>();

      mArVertex3f.Clear();
      mArNormal3f.Clear();
      mArEdgeLines.Clear();
      mArFace.Clear();
      mArVertex3fTemp.Clear();
    }





    /// //////////////////////////////////////////////////

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
