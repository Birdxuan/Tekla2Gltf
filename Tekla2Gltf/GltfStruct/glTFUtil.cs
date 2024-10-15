using System;
using System.Collections.Generic;
using System.IO;
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
      var vecAccessor = addAccessor(gltf.bufferViews.Count - 1, 0, ComponentType.FLOAT, v3ds.Count / 3, AccessorType.VEC3);
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
      // Get index data
      var indices = bufferData.indexBuffer;

      // Initialize byte offset
      var byteOffset = 0;

      // Calculate new byteOffset if Buffer View already exists
      if (gltf.bufferViews.Count > 0)
      {
        var lastBufferView = gltf.bufferViews[gltf.bufferViews.Count - 1];
        byteOffset = lastBufferView.byteLength + lastBufferView.byteOffset;
      }

      // Buffer index, assuming it is 0
      var bufferIndex = 0;

      // Create a new Buffer View to store index data
      var indexSize = bufferData.indexMax > 65535 ? 4 : 2;
      var indexBufferView = addBufferView(bufferIndex, byteOffset, indexSize * indices.Count);
      indexBufferView.target = Targets.ELEMENT_ARRAY_BUFFER;

      // Add the new Buffer View to the glTF's bufferViews list
      gltf.bufferViews.Add(indexBufferView);

      var componentType = bufferData.indexMax > 65535 ? ComponentType.UNSIGNED_INT : ComponentType.UNSIGNED_SHORT;
      // Create a new Accessor to describe the data format and range of the index data
      var indexAccessor = addAccessor(gltf.bufferViews.Count - 1, 0, componentType, indices.Count, AccessorType.SCALAR);

      // Add the new Accessor to the glTF's accessors list
      gltf.accessors.Add(indexAccessor);
    }

    public static void addUvBufferViewAndAccessor(GLTF gltf, glTFBinaryData bufferData)
    {
      // Get UV coordinate data
      var uvs = bufferData.uvBuffer;

      // Initialize byte offset
      var byteOffset = 0;

      // Calculate new byteOffset if Buffer View already exists
      if (gltf.bufferViews.Count > 0)
      {
        var lastBufferView = gltf.bufferViews[gltf.bufferViews.Count - 1];
        byteOffset = lastBufferView.byteLength + lastBufferView.byteOffset;
      }

      // Buffer index, assuming it is 0
      var bufferIndex = 0;

      // Create a new Buffer View to store UV coordinate data
      var vec3View = addBufferView(bufferIndex, byteOffset, 4 * uvs.Count);
      vec3View.target = Targets.ARRAY_BUFFER;

      // Add the new Buffer View to the glTF's bufferViews list
      gltf.bufferViews.Add(vec3View);

      // Create a new Accessor to describe the data format and range of the UV coordinates
      var vecAccessor = addAccessor(gltf.bufferViews.Count - 1, 0, ComponentType.FLOAT, uvs.Count / 2, AccessorType.VEC2);

      // Add the new Accessor to the glTF's accessors list
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

    public static void Align(Stream stream, byte pad = 0)
    {
      var count = 3 - ((stream.Position - 1) & 3);
      while (count != 0)
      {
        stream.WriteByte(pad);
        count--;
      }
    }
  }
}
