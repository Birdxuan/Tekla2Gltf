#define USE_TEKLA_GEO_INFO

using System;
using System.Windows.Forms;


namespace ExportTekla2Gltf
{
  public partial class ExportUI : Form
  {
    UIData uiData = UIData.InstanceUI;
    public ExportUI()
    {
      InitializeComponent();
      Init();
    }

    private void Init()
    {
      DestinationPathFile.Text = @"D:\test.gltf";
      uiData.m_strSavePath = @"D:\test.gltf";
    }

    private void SelectPath_Click(object sender, EventArgs e)
    {
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.Filter = "GLTF Files (*.gltf)|*.gltf|GLB Files (*.glb)|*.glb";
      saveFileDialog.DefaultExt = "gltf"; // Default extension
      saveFileDialog.AddExtension = true; // Automatically add the extension

      if (saveFileDialog.ShowDialog() == DialogResult.OK)
      {
        uiData.m_strSavePath = saveFileDialog.FileName;
        DestinationPathFile.Text = uiData.m_strSavePath;
      }
    }

    private void Start2Gltf_Click(object sender, EventArgs e)
    {
#if USE_TEKLA_GEO_INFO
      TeklaGeoInfo teklaGeoInfo = new TeklaGeoInfo();
      Logger.GetInstance().Info("GeoData Function -------------------------------------------------------------------------------------------- >  Start");
      teklaGeoInfo.GeoData();
      Logger.GetInstance().Info("GeoData Function -------------------------------------------------------------------------------------------- >  PackStart");
      teklaGeoInfo.PackageGeoData();
      Logger.GetInstance().Info("GeoData Function -------------------------------------------------------------------------------------------- >  WriteStart");
      teklaGeoInfo.WriteGltf();
#else
      TeklaAPIHelpTest.testVVV();

#endif
    }
  }
}
