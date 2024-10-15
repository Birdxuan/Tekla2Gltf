using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportTekla2Gltf
{
  public class glTFUtil
  {
    public static void addVec3BufferViewAndAccessor(GLTF gltf, glTFBinaryData bufferData)
    {
      var v3ds = bufferData.vertexBuffer;
      var byteOffset = 0;
      if (gltf.bufferViews.Count > 0)
      {
        byteOffset = gltf.bufferViews[gltf.bufferViews.Count - 1].byteLength + gltf.bufferViews[gltf.bufferViews.Count - 1].byteOffset;
      }
      var bufferIndex = 0;
      var vec3View = addBufferView(bufferIndex, byteOffset, 4 * v3ds.Count);
      vec3View.target = Targets.ARRAY_BUFFER;
      gltf.bufferViews.Add(vec3View);
      var vecAccessor =addAccessor(gltf.bufferViews.Count - 1, 0, ComponentType.FLOAT, v3ds.Count / 3, AccessorType.VEC3);
      var minAndMax = GetVec3MinMax(v3ds);
      vecAccessor.min = new List<double>() { minAndMax[0], minAndMax[1], minAndMax[2] };
      vecAccessor.max = new List<double>() { minAndMax[3], minAndMax[4], minAndMax[5] };
      gltf.accessors.Add(vecAccessor);
    }

    public static void addNormalBufferViewAndAccessor(GLTF gltf, glTFBinaryData bufferData)
    {
      var v3ds = bufferData.normalBuffer;
      var byteOffset = 0;
      if (gltf.bufferViews.Count > 0)
      {
        byteOffset = gltf.bufferViews[gltf.bufferViews.Count - 1].byteLength + gltf.bufferViews[gltf.bufferViews.Count - 1].byteOffset;
      }
      var bufferIndex = 0;
      var vec3View = addBufferView(bufferIndex, byteOffset, 4 * v3ds.Count);
      vec3View.target = Targets.ARRAY_BUFFER;
      gltf.bufferViews.Add(vec3View);
      var vecAccessor = addAccessor(gltf.bufferViews.Count - 1, 0, ComponentType.FLOAT, v3ds.Count / 3, AccessorType.VEC3);
      gltf.accessors.Add(vecAccessor);
    }

    public static void addIndexsBufferViewAndAccessor(GLTF gltf, glTFBinaryData bufferData)
    {
      // 获取索引数据
      var indices = bufferData.indexBuffer;

      // 初始化字节偏移量
      var byteOffset = 0;

      // 如果已有 Buffer View，则计算新的 byteOffset
      if (gltf.bufferViews.Count > 0)
      {
        var lastBufferView = gltf.bufferViews[gltf.bufferViews.Count - 1];
        byteOffset = lastBufferView.byteLength + lastBufferView.byteOffset;
      }

      // Buffer 索引，假设为 0
      var bufferIndex = 0;

      // 创建一个新的 Buffer View，用于存储索引数据
      var indexBufferView = addBufferView(bufferIndex, byteOffset, 2 * indices.Count);
      indexBufferView.target = Targets.ELEMENT_ARRAY_BUFFER;

      // 将新的 Buffer View 添加到 glTF 的 bufferViews 列表中
      gltf.bufferViews.Add(indexBufferView);

      // 创建一个新的 Accessor，用于描述索引数据的数据格式和范围
      var indexAccessor = addAccessor(gltf.bufferViews.Count - 1, 0, ComponentType.UNSIGNED_SHORT, indices.Count, AccessorType.SCALAR);

      // 将新的 Accessor 添加到 glTF 的 accessors 列表中
      gltf.accessors.Add(indexAccessor);
    }


    public static void addUvBufferViewAndAccessor(GLTF gltf, glTFBinaryData bufferData)
    {
      // 获取 UV 坐标数据
      var uvs = bufferData.uvBuffer;

      // 初始化字节偏移量
      var byteOffset = 0;

      // 如果已有 Buffer View，则计算新的 byteOffset
      if (gltf.bufferViews.Count > 0)
      {
        var lastBufferView = gltf.bufferViews[gltf.bufferViews.Count - 1];
        byteOffset = lastBufferView.byteLength + lastBufferView.byteOffset;
      }

      // Buffer 索引，假设为 0
      var bufferIndex = 0;

      // 创建一个新的 Buffer View，用于存储 UV 坐标数据
      var vec3View = addBufferView(bufferIndex, byteOffset, 4 * uvs.Count);
      vec3View.target = Targets.ARRAY_BUFFER;

      // 将新的 Buffer View 添加到 glTF 的 bufferViews 列表中
      gltf.bufferViews.Add(vec3View);

      // 创建一个新的 Accessor，用于描述 UV 坐标的数据格式和范围
      var vecAccessor = addAccessor(gltf.bufferViews.Count - 1, 0, ComponentType.FLOAT, uvs.Count / 2, AccessorType.VEC2);

      // 将新的 Accessor 添加到 glTF 的 accessors 列表中
      gltf.accessors.Add(vecAccessor);
    }

    public static glTFBufferView addBufferView(int bufferIndex, int byteOffset, int byteLength)
    {
      var bufferView = new glTFBufferView();
      bufferView.buffer = bufferIndex;
      bufferView.byteOffset = byteOffset;
      bufferView.byteLength = byteLength;
      return bufferView;
    }

    public static glTFAccessor addAccessor(int bufferView, int byteOffset, ComponentType componentType, int count, string type)
    {
      var accessor = new glTFAccessor();
      accessor.bufferView = bufferView;
      accessor.byteOffset = byteOffset;
      accessor.componentType = componentType;
      accessor.count = count;
      accessor.type = type;
      return accessor;
    }

    public static float[] GetVec3MinMax(List<float> vec3)
    {
      List<float> xValues = new List<float>();
      List<float> yValues = new List<float>();
      List<float> zValues = new List<float>();
      for (int i = 0; i < vec3.Count; i++)
      {
        if ((i % 3) == 0) xValues.Add(vec3[i]);
        if ((i % 3) == 1) yValues.Add(vec3[i]);
        if ((i % 3) == 2) zValues.Add(vec3[i]);
      }
      float maxX = xValues.Max();
      float minX = xValues.Min();
      float maxY = yValues.Max();
      float minY = yValues.Min();
      float maxZ = zValues.Max();
      float minZ = zValues.Min();
      return new float[] { minX, minY, minZ, maxX, maxY, maxZ };
    }






  }
}
