using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportTekla2Gltf
{

  public class gltfExport
  {
    private GLTF glTF;

    private List<glTFBinaryData> allBinaryDatas;


    private Dictionary<string, glTFBinaryData> curMapBinaryData = new Dictionary<string, glTFBinaryData>();
    public gltfExport()
    {
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


      if (!curMapBinaryData.ContainsKey("test"))
      {
        curMapBinaryData.Add("test", new glTFBinaryData());
      }

    }


    public void Mesh()
    {
      var currentGeometry = curMapBinaryData["test"];



    }
  }
}
