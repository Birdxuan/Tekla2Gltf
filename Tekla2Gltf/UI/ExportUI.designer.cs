
namespace ExportTekla2Gltf
{
  partial class ExportUI
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.DestinationPathFile = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.SelectPath = new System.Windows.Forms.Button();
      this.Start2Gltf = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // DestinationPathFile
      // 
      this.DestinationPathFile.Location = new System.Drawing.Point(83, 36);
      this.DestinationPathFile.Name = "DestinationPathFile";
      this.DestinationPathFile.Size = new System.Drawing.Size(474, 21);
      this.DestinationPathFile.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 39);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(65, 12);
      this.label1.TabIndex = 1;
      this.label1.Text = "Gltf  Path";
      // 
      // SelectPath
      // 
      this.SelectPath.Location = new System.Drawing.Point(560, 36);
      this.SelectPath.Name = "SelectPath";
      this.SelectPath.Size = new System.Drawing.Size(36, 23);
      this.SelectPath.TabIndex = 2;
      this.SelectPath.Text = "...";
      this.SelectPath.UseVisualStyleBackColor = true;
      this.SelectPath.Click += new System.EventHandler(this.SelectPath_Click);
      // 
      // Start2Gltf
      // 
      this.Start2Gltf.Location = new System.Drawing.Point(256, 102);
      this.Start2Gltf.Name = "Start2Gltf";
      this.Start2Gltf.Size = new System.Drawing.Size(75, 23);
      this.Start2Gltf.TabIndex = 3;
      this.Start2Gltf.Text = "Start";
      this.Start2Gltf.UseVisualStyleBackColor = true;
      this.Start2Gltf.Click += new System.EventHandler(this.Start2Gltf_Click);
      // 
      // ExportUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(596, 146);
      this.Controls.Add(this.Start2Gltf);
      this.Controls.Add(this.SelectPath);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.DestinationPathFile);
      this.Name = "ExportUI";
      this.Text = "Tekla2Gltf";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox DestinationPathFile;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button SelectPath;
    private System.Windows.Forms.Button Start2Gltf;
  }
}