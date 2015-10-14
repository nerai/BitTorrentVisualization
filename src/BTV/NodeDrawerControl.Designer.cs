namespace BitTorrentVisualization
{
	partial class NodeDrawerControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.SuspendLayout ();
			// 
			// NodeDrawerControl
			// 
			this.BackColor = System.Drawing.Color.Black;
			this.DoubleBuffered = true;
			this.Name = "NodeDrawerControl";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler (this.NodeDrawerControl_KeyDown);
			this.ResumeLayout (false);

		}

		#endregion

	}
}
