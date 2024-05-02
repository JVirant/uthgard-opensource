using OpenTK.WinForms;

namespace DaocModelViewer
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.glControl = new OpenTK.WinForms.GLControl();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.glControl);
			this.splitContainer1.Size = new System.Drawing.Size(800, 450);
			this.splitContainer1.SplitterDistance = 266;
			this.splitContainer1.TabIndex = 0;
			// 
			// glControl
			// 
			this.glControl.API = OpenTK.Windowing.Common.ContextAPI.OpenGL;
			this.glControl.APIVersion = new System.Version(3, 3, 0, 0);
			this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glControl.Flags = OpenTK.Windowing.Common.ContextFlags.Default;
			this.glControl.IsEventDriven = true;
			this.glControl.Location = new System.Drawing.Point(0, 0);
			this.glControl.Name = "glControl";
			this.glControl.Profile = OpenTK.Windowing.Common.ContextProfile.Core;
			this.glControl.Size = new System.Drawing.Size(530, 450);
			this.glControl.TabIndex = 0;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.splitContainer1);
			this.Name = "MainForm";
			this.Text = "DAoC Model Viewer";
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private SplitContainer splitContainer1;
		private GLControl glControl;
	}
}