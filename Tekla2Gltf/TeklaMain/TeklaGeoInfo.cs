using g3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Solid;
using TriangleNet.Geometry;
using Point = Tekla.Structures.Geometry3d.Point;
using Polygon = TriangleNet.Geometry.Polygon;
using Tekla.Structures.Model.UI;
using Tekla.Structures;
using TriangleNet.Topology;

namespace ExportTekla2Gltf
{
  class TeklaAPIHelpTest
  {

    /// ///////////////////////////////////////////////////////////////////Test///////////////////////////////////////////////////

    // This is a test function used to test some API-related functions
    public static void testVVV()
    {
      // Initialize the model and prepare to establish a connection
      Model model = new Model();

      // Safety check
      if (!model.GetConnectionStatus())
      {
        return;
      }

      // Input OriginPartId
      int originPartId = 3502544; // The OriginPartId to check

      // Get all model data; this part has issues [TODO]^-^
      /*
          * Some model data cannot be retrieved under hidden views
          */
      ModelObjectEnumerator allObjects = model.GetModelObjectSelector().GetAllObjects();

      while (allObjects.MoveNext())
      {
        ModelObject obj = allObjects.Current;

        if (obj is Part)
        {
          Part part = obj as Part;

          int currentOriginPartId = part.Identifier.ID;

          if (currentOriginPartId == originPartId)
          {
            part.Select();

            // Focus 
            Solid solid = part.GetSolid();
            Point minPoint = solid.MinimumPoint;
            Point maxPoint = solid.MaximumPoint;

            // Common cube bounding box
            AABB aabb = new AABB(minPoint, maxPoint);

            // Only display the selected object
            // Create a new visualization object
            Color color = new Color();
            ModelObjectVisualization.GetRepresentation(part, ref color);

            var it = ViewHandler.GetAllViews();
            while (it.MoveNext())
            {
              var ititem = it.Current;
              if (ititem.Name == "3d")
              {
                Color colorset = new Color(1, 0, 0, 1);
                List<Identifier> identifiers = new List<Identifier>();
                identifiers.Add(part.Identifier);
                ModelObjectVisualization.SetTemporaryState(identifiers, colorset);

                ViewHandler.ZoomToBoundingBox(ititem, aabb);
                break;
              }
            }
          }
        }
      }
      model.CommitChanges();
    }

  }


  /// ///////////////////////////////////////////////////////////////////Step0: geometry3d ///////////////////////////////////////////////////
  public class TeklaGeoInfo
  {
    public bool GeoData()
    {
      bool bre = false;
      Model currentModel = new Model();
      if (currentModel.GetConnectionStatus())
      {
        ModelObjectEnumerator enumeratorFrame = currentModel.GetModelObjectSelector().GetAllObjects();

        while (enumeratorFrame.MoveNext())
        {
          try
          {
            if (enumeratorFrame.Current is Part || //Normal
                enumeratorFrame.Current is BoltArray)//Bolt
            {
              GetMembType(enumeratorFrame.Current);
            }
          }
          catch (Exception ex)
          {
            string str = ex.ToString();
            Logger.GetInstance().Error("GeoData" + str);
          }
        }
      }

      return bre;
    }


    private void GetMembType(Tekla.Structures.Model.Object obj)
    {
      bool bre = false;
      string strGuid = Guid.NewGuid().ToString();

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
        bre = GetData(solid, strGuid);
      }

      if (bre)
      {
        TeklaMaterial(obj, strGuid);
      }

    }

    private bool GetData(Solid solid, string strGuid)
    {
      bool bre = false;

      if (solid != null)
      {
        FaceEnumerator faceEnumerator = solid.GetFaceEnumerator();
        while (faceEnumerator.MoveNext())
        {
          // Iterate through faces
          Face face = faceEnumerator.Current;
          Vector normal = face.Normal; normal.Normalize();

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

                while (vertexEnumerator.MoveNext())
                {
                  if (vertexEnumerator.Current is Point pt)
                  {
                    points.Add(pt);
                  }
                }
              }
            }
          }
          bool breTemp = Triangulate(points, normal, face, strGuid);
          if (breTemp)
          {
            bre = breTemp;
          }
        }

      }

      return bre;
    }

    /// <summary>
    ///  Core function
    /// </summary>
    /// <param name="points"></param>
    /// <param name="normal"></param>
    /// <param name="face"></param>
    /// <param name="strGuid"></param>
    /// <returns></returns>
    private bool Triangulate(List<Point> points, Vector normal, Face face, string strGuid)
    {
      bool breMater = true;

      ///
      try
      {
        Dictionary<Vector2d, KeyValuePair<int, Point>> Dic2Dim3 = new Dictionary<Vector2d, KeyValuePair<int, Point>>();

        List<Vector2d> lst2D = new List<Vector2d>();
        Vector3d point3D0 = new Vector3d(points[0].X, points[0].Y, points[0].Z);
        for (int i = 0; i < points.Count; i++)
        {
          Vector3d point3D = new Vector3d(points[i].X, points[i].Y, points[i].Z);
          Vector3d planePoint = point3D0;
          Vector3d planeNormal = new Vector3d(normal.X, normal.Y, normal.Z); // The plane is the XY plane


          // Project 3D points onto the plane
          Vector2d point2D = ProjectPointToPlane(point3D, planePoint, planeNormal);
          lst2D.Add(point2D);
          Dic2Dim3[point2D] = new KeyValuePair<int, Point>(i, points[i]);
        }

        if (Dic2Dim3.Count == 1)
        {
          // Investigate what went wrong here; it seems to be related to geometry data of the perforation; haven't delved into it much [TODO] ^-^
          //Logger.GetInstance().Warning("Face Number  = 1--->" + face.OriginPartId + "<OriginPartId---GUID>" + face.OriginPartId.GUID.ToString());
          return false;
        }

        if (!curMapBinaryData.ContainsKey(strGuid))
        {
          curMapBinaryData.Add(strGuid, new glTFBinaryData());
        }


        glTFBinaryData currentGeometry = curMapBinaryData[strGuid];
        var index = currentGeometry.vertexBuffer.Count / 3;

        bool bre = IsConvex(lst2D, normal);

        // Use Triangle.NET for Delaunay triangulation
        {
          var polygon = new Polygon();
          for (int i = 0; i < lst2D.Count; i++)
          {
            polygon.Add(new Vertex(lst2D[i].x, lst2D[i].y));
          }

          if (bre)
          {
            var mesh = polygon.Triangulate();
            foreach (Triangle tri in mesh.Triangles)
            {
              AddTriangleToGeometry(tri, index, currentGeometry);
            }
          }
          else
          {
            var mesh = polygon.Triangulate();
            var validTriangles = mesh.Triangles.Where(t => IsTriangleInside(t, polygon));
            foreach (Triangle tri in validTriangles)
            {
              AddTriangleToGeometry(tri, index, currentGeometry);
            }
          }

          //
          foreach (var pt in points)
          {
            double x = pt.X;
            double y = pt.Y;
            double z = pt.Z;
            currentGeometry.vertexBuffer.Add((float)x);
            currentGeometry.vertexBuffer.Add((float)y);
            currentGeometry.vertexBuffer.Add((float)z);

            normal.Normalize();

            currentGeometry.normalBuffer.Add((float)normal.X);
            currentGeometry.normalBuffer.Add((float)normal.Y);
            currentGeometry.normalBuffer.Add((float)normal.Z);

          }
        }

        return breMater;
      }
      catch (Exception ex)
      {
        string str = ex.ToString();
        Logger.GetInstance().Error("Triangulate" + str);
        return false;
      }
    }

    private void AddTriangleToGeometry(Triangle tri, int index, glTFBinaryData currentGeometry)
    {
      for (int i = 0; i < 3; i++)
      {
        int vertexIndex = tri.GetVertex(i).ID + index;
        currentGeometry.indexBuffer.Add(vertexIndex);
        currentGeometry.indexMax = Math.Max(currentGeometry.indexMax ?? 0, vertexIndex);
      }
    }


    public Vector2d ProjectPointToPlane(Vector3d point, Vector3d planePoint, Vector3d planeNormal)
    {
      // Calculate the projection of a point onto the plane
      Vector3d pointToPlane = point - planePoint;
      double distance = pointToPlane.Dot(planeNormal);
      Vector3d projectedPoint3D = point - distance * planeNormal;

      // Calculate two basis vectors on the plane
      Vector3d u = planeNormal.Cross(new Vector3d(1, 0, 0));
      if (u.LengthSquared == 0)
      {
        u = planeNormal.Cross(new Vector3d(0, 1, 0));
      }
      u.Normalize();

      Vector3d v = planeNormal.Cross(u);
      v.Normalize();

      // Convert the projected 3D point to a 2D point
      Vector2d projectedPoint2D = new Vector2d(projectedPoint3D.Dot(u), projectedPoint3D.Dot(v));

      return projectedPoint2D;
    }

    /// <summary>
    /// Convexity check, the algorithm uses the sum of interior angles (n - 2) * Math.PI to determine convexity
    /// </summary>
    /// <param name="projectedPoints"></param>
    /// <param name="normal"></param>
    /// <returns></returns>
    private bool IsConvex(List<Vector2d> projectedPoints, Vector normal)
    {
      if (projectedPoints.Count < 3)
      {
        throw new ArgumentException("A polygon must have at least 3 points.");
      }

      double angleSum = 0;
      int n = projectedPoints.Count;

      for (int i = 0; i < n; i++)
      {
        Vector2d prev = projectedPoints[(i - 1 + n) % n];
        Vector2d curr = projectedPoints[i];
        Vector2d next = projectedPoints[(i + 1) % n];

        Vector2d v1 = prev - curr;
        Vector2d v2 = next - curr;

        double angle = Math.Acos(v1.Dot(v2) / (v1.Length * v2.Length));
        angleSum += angle;
      }

      // Considering floating-point precision, using a small tolerance value
      double expectedSum = (n - 2) * Math.PI;
      return Math.Abs(angleSum - expectedSum) < 1e-6;
    }

    /// <summary>
    /// Validity check, solves bugs related to concave polygons by filtering
    /// </summary>
    /// <param name="triangle"></param>
    /// <param name="polygon"></param>
    /// <returns></returns>
    private bool IsTriangleInside(ITriangle triangle, Polygon polygon)
    {
      // Get the three vertices of the triangle
      var p1 = triangle.GetVertex(0);
      var p2 = triangle.GetVertex(1);
      var p3 = triangle.GetVertex(2);

      // Check if the centroid of the triangle is inside the polygon
      var centerX = (p1.X + p2.X + p3.X) / 3;
      var centerY = (p1.Y + p2.Y + p3.Y) / 3;

      if (!IsPointInPolygon(centerX, centerY, polygon))
      {
        return false;
      }

      // Check if the edges of the triangle intersect with the edges of the polygon
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
        return false; // Line segments are parallel
      }
      else
      {
        double lambda = ((b2.Y - b1.Y) * (b2.X - a1.X) + (b1.X - b2.X) * (b2.Y - a1.Y)) / det;
        double gamma = ((a1.Y - a2.Y) * (b2.X - a1.X) + (a2.X - a1.X) * (b2.Y - a1.Y)) / det;
        return (0 < lambda && lambda < 1) && (0 < gamma && gamma < 1);
      }
    }

    /// <summary>
    /// RGB 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="curMaterialName"></param>
    private void TeklaMaterial(Tekla.Structures.Model.Object obj, string curMaterialName)
    {
      try
      {
        Color color = new Color();
        ModelObjectVisualization.GetRepresentation(obj as ModelObject, ref color);

        if (!MapMaterial.ContainsKey(curMaterialName))
        {
          glTFMaterial gl_mat = new glTFMaterial();
          gl_mat.name = curMaterialName;
          gl_mat.index = glTF.materials.Count;
          glTFPBR pbr = new glTFPBR();
          pbr.baseColorFactor = new List<double>() { color.Red, color.Green, color.Blue, color.Transparency };
          pbr.metallicFactor = 0f;
          pbr.roughnessFactor = 1f;
          gl_mat.pbrMetallicRoughness = pbr;
          glTF.materials.Add(gl_mat);
          gl_mat.index = MapMaterial.Count;
          MapMaterial.Add(curMaterialName, gl_mat);

        }
      }
      catch (Exception ex)
      {
        string str = ex.ToString();
        Logger.GetInstance().Error("TeklaMaterial" + str);
      }
    }


    /// /////////////////////////////////////////////////////////Step1:  Write///////////////////////////////////////////

    private GLTF glTF;

    private List<glTFBinaryData> allBinaryDatas;

    string gltfOutDir = "";

    private Dictionary<string, glTFBinaryData> curMapBinaryData = new Dictionary<string, glTFBinaryData>();

    private Dictionary<string, glTFMaterial> MapMaterial = new Dictionary<string, glTFMaterial>();
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
      glTF.materials = new List<glTFMaterial>();
      var scence = new glTFScene();
      scence.nodes = new List<int>() { 0 };
      glTF.scenes.Add(scence);
      glTFNode root = new glTFNode();
      root.name = "root";
      root.children = new List<int>();
      // Set the Y-axis upwards
      root.matrix = new List<double>()
            {
                0.3048, 0.0,0.0, 0.0,
                0.0,0.0, -0.3048, 0.0,
                0.0,0.3048,0.0,0.0,
                0.0,0.0,0.0, 1.0
            };
      glTF.nodes.Add(root);
      allBinaryDatas = new List<glTFBinaryData>();
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
          primative.material = MapMaterial[key].index;
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
      else if (fileExtension == ".glb")
      {
        using (var fileStream = File.Create(UIData.InstanceUI.m_strSavePath))
        using (var writer = new BinaryWriter(fileStream))
        {
          newbuffer.uri = null;
          writer.Write(GLB.Magic);
          writer.Write(GLB.Version);
          var chunksPosition = writer.BaseStream.Position;
          writer.Write(0U);
          var jsonChunkPosition = writer.BaseStream.Position;
          writer.Write(0U);
          writer.Write(GLB.ChunkFormatJson);
          using (var streamWriter = new StreamWriter(writer.BaseStream, new UTF8Encoding(false, true), 1024, true))
          using (var jsonTextWriter = new JsonTextWriter(streamWriter))
          {
            JObject json = JObject.Parse(glTF.toJson());
            json.WriteTo(jsonTextWriter);
          }
          glTFUtil.Align(writer.BaseStream, 0x20);
          var jsonChunkLength = checked((uint)(writer.BaseStream.Length - jsonChunkPosition)) - GLB.ChunkHeaderLength;
          writer.BaseStream.Seek(jsonChunkPosition, SeekOrigin.Begin);
          writer.Write(jsonChunkLength);
          byte[] data = memoryStream.ToArray();
          writer.BaseStream.Seek(0, SeekOrigin.End);
          var binChunkPosition = writer.BaseStream.Position;
          writer.Write(0);
          writer.Write(GLB.ChunkFormatBin);
          foreach (var b in data)
          {
            writer.Write(b);
          }
          glTFUtil.Align(writer.BaseStream, 0x20);
          var binChunkLength = checked((uint)(writer.BaseStream.Length - binChunkPosition)) - GLB.ChunkHeaderLength;
          writer.BaseStream.Seek(binChunkPosition, SeekOrigin.Begin);
          writer.Write(binChunkLength);
          var length = checked((uint)writer.BaseStream.Length);
          writer.BaseStream.Seek(chunksPosition, SeekOrigin.Begin);
          writer.Write(length);
        }
      }
      MessageBox.Show("Saved successfully.");

    }

  }
}
