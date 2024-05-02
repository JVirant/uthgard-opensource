namespace HMapEdit.Forms
{
	partial class NIFRenderControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur de composants

		/// <summary> 
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.renderLoop = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// renderLoop
			// 
			this.renderLoop.Enabled = true;
			this.renderLoop.Interval = 8;
			this.renderLoop.Tick += new System.EventHandler(this._Render);
			// 
			// NIFRenderControl
			// 
			this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
			this.DoubleBuffered = true;
			this.Name = "NIFRenderControl";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer renderLoop;
	}
}
