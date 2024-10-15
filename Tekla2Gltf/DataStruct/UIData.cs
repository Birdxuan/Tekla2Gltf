using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExportTekla2Gltf
{
  public class UIData
  {
    private static UIData _instanceUI;

    public static UIData InstanceUI
    {
      get
      {
        if (_instanceUI == null)
        {
          _instanceUI = new UIData();
        }
        return _instanceUI;
      }
    }
    public string m_strSavePath { get; set; }

  }
}
