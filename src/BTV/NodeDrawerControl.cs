using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BitTorrentVisualization
{
	public partial class NodeDrawerControl : UserControl
	{
		private readonly System.Timers.Timer _Step;
		private readonly NodeManager _NodeManager;

		public NodeDrawerControl ()
		{
			InitializeComponent ();

			_NodeManager = new NodeManager ();

			_Step = new System.Timers.Timer (1000 / 60);
			_Step.Enabled = true;
			_Step.Elapsed += (s, e) => {
				_NodeManager.Step ();
				Invalidate ();
			};
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
			var g = e.Graphics;

			DrawPackets (g);
			DrawNodes (g);
			DrawHelpText (g);
			DrawAvailability (g);
			DrawStatisticsText (g);
		}

		private void DrawStatisticsText (Graphics g)
		{
			var s = _NodeManager.GetStatistics ();
			var box = g.MeasureString (s, DefaultFont);
			g.DrawString (
				s,
				DefaultFont,
				Brushes.Gray,
				0,
				ClientRectangle.Height - box.Height);
		}

		private void DrawAvailability (Graphics g)
		{
			var avail = new Dictionary<int, float> ();
			foreach (var pair in _NodeManager.GlobalAvailability ().PieceAvailability) {
				avail[pair.Key] = (float) pair.Value / _NodeManager.Nodes.Count ();
			}

			var rw = ClientRectangle.Width * 0.25f;
			var rh = rw / 5 * (float) Math.Sqrt (_NodeManager.Nodes.Count ());
			var rectPieces = new RectangleF (ClientRectangle.Width - rw, 0, rw, rh);
			Node.DrawPieceRect (g, rectPieces, avail, 100f * avail.Average (x => x.Value), true);
		}

		private void DrawNodes (Graphics g)
		{
			foreach (Node node in _NodeManager.Nodes.ToArray ()) {
				node.DrawSelf (g, ClientRectangle);
			}
		}

		private void DrawPackets (Graphics g)
		{
			foreach (Packet packet in _NodeManager.Packets.ToArray ()) {
				packet.DrawSelf (g, ClientRectangle);
			}
		}

		private void DrawHelpText (Graphics g)
		{
			var s =
				"BitTorrent Visualization" + Environment.NewLine +
				"by Sebastian Heuchler (dev@kikashi.net)" + Environment.NewLine +
				"" + Environment.NewLine +
				"S\tcreate seed" + Environment.NewLine +
				"L\tcreate leech" + Environment.NewLine +
				"P\tcreate peer" + Environment.NewLine +
				"R\tremove node" + Environment.NewLine +
				"C\tclear all nodes" + Environment.NewLine +
				"T\ttoggle inner circle (Is:" + (_NodeManager.DistinctInnerCircle ? "on" : "off") + ")" + Environment.NewLine +
				"D\ttoggle demo mode (Is:" + (_NodeManager.IsInDemoMode ? "on" : "off") + ")" + Environment.NewLine +
				"";
			g.DrawString (
				s,
				DefaultFont,
				Brushes.Gray,
				0,
				0);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			Invalidate ();
		}

		private void NodeDrawerControl_KeyDown (object sender, KeyEventArgs e)
		{
			switch (e.KeyCode) {
				case Keys.S:
					_NodeManager.AddNewNode ().MakeSeed (false);
					break;

				case Keys.L:
					_NodeManager.AddNewNode ().MakeSeed (true);
					break;

				case Keys.P:
					_NodeManager.AddNewNode ();
					break;

				case Keys.C:
					_NodeManager.ClearNodes ();
					break;

				case Keys.D:
					_NodeManager.IsInDemoMode = !_NodeManager.IsInDemoMode;
					break;

				case Keys.T:
					_NodeManager.DistinctInnerCircle = !_NodeManager.DistinctInnerCircle;
					break;

				case Keys.R:
					_NodeManager.RemoveNode ();
					break;
			}
		}
	}
}
