namespace BitTorrentVisualization
{
	partial class NodeDisplayForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.nodeDrawer1 = new BitTorrentVisualization.NodeDrawerControl();
			this.SuspendLayout();
			// 
			// nodeDrawer1
			// 
			this.nodeDrawer1.BackColor = System.Drawing.Color.Black;
			this.nodeDrawer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nodeDrawer1.Location = new System.Drawing.Point(0, 0);
			this.nodeDrawer1.Name = "nodeDrawer1";
			this.nodeDrawer1.Size = new System.Drawing.Size(600, 600);
			this.nodeDrawer1.TabIndex = 0;
			// 
			// NodeDisplayForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(600, 600);
			this.Controls.Add(this.nodeDrawer1);
			this.Name = "NodeDisplayForm";
			this.Text = "BitTorrent Visualization";
			this.ResumeLayout(false);

		}

		#endregion

		private NodeDrawerControl nodeDrawer1;
	}
}