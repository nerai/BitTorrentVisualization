using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace BitTorrentVisualization
{
	public class Node
	{
		private readonly Random _Random = new Random ();
		private readonly NodeManager _NodeManager;

		public Node (NodeManager nodeManager)
		{
			_NodeManager = nodeManager;
		}

		public void Step ()
		{
			StepPosition ();
			StepColor ();
			StepPieces ();
			_StepsUntilReadyToSend--;
			_StepsUntilReadyToRequest--;

			if (_Congestion > 0) {
				_Congestion--;
			}
		}

		private float _ActualPositionX = 0.5f;
		private float _ActualPositionY = 0.5f;

		private float _TargetPositionX = 0.5f;
		private float _TargetPositionY = 0.5f;

		public float PositionX
		{
			get
			{
				return _ActualPositionX;
			}
			set
			{
				_TargetPositionX = value;
			}
		}

		public float PositionY
		{
			get
			{
				return _ActualPositionY;
			}
			set
			{
				_TargetPositionY = value;
			}
		}

		private void StepPosition ()
		{
			const float PositionAdjustmentFactor = 0.02f;
			_ActualPositionX += (_TargetPositionX - _ActualPositionX) * PositionAdjustmentFactor;
			_ActualPositionY += (_TargetPositionY - _ActualPositionY) * PositionAdjustmentFactor;
		}

		private float _ActualR;
		private float _ActualG;
		private float _ActualB;

		private float _TargetR;
		private float _TargetG;
		private float _TargetB;

		public Color Color
		{
			get
			{
				return Color.FromArgb (
					127,
					(int) _ActualR,
					(int) _ActualG,
					(int) _ActualB);
			}
			set
			{
				_TargetR = value.R;
				_TargetG = value.G;
				_TargetB = value.B;
			}
		}

		private void StepColor ()
		{
			const float ColorAdjustmentFactor = 0.005f;
			_ActualR += (_TargetR - _ActualR) * ColorAdjustmentFactor;
			_ActualG += (_TargetG - _ActualG) * ColorAdjustmentFactor;
			_ActualB += (_TargetB - _ActualB) * ColorAdjustmentFactor;
		}

		public const int PiecesCount = 100;
		private readonly bool[] _HasPiece = new bool[PiecesCount];
		private readonly int[] _HasPartOfPiece = new int[PiecesCount];
		private readonly bool[] _RequestedPiece = new bool[PiecesCount];

		private readonly int _RequestInterval = 30;
		private readonly int _SendInterval = 30;
		private int _StepsUntilReadyToSend = 1;
		private int _StepsUntilReadyToRequest = 1;

		public void MakeSeed (bool partial)
		{
			for (int i = 0; i < PiecesCount; i++) {
				if (!partial || _Random.NextDouble () < 0.5) {
					AddPiece (i);
					_HasPartOfPiece[i] = Packet.PartsPerPiece;
				}
			}
		}

		public bool HasPiece (int index)
		{
			if (index < 0 || index >= PiecesCount) {
				throw new ArgumentOutOfRangeException ();
			}

			return _HasPiece[index];
		}

		public bool RequestPiece (int index, Node target)
		{
			if (!HasPiece (index)) {
				return false;
			}

			if (_StepsUntilReadyToSend > 0) {
				return false;
			}

			_StepsUntilReadyToSend = _SendInterval;

			Packet p = new Packet (this, target, index);
			_NodeManager.AddPacket (p);

			return true;
		}

		private void StepPieces ()
		{
			if (_StepsUntilReadyToRequest > 0) {
				return;
			}

			var missing = Enumerable
				.Range (0, PiecesCount)
				.Where (i => !_HasPiece[i] && !_RequestedPiece[i])
				.ToList ();
			if (!missing.Any ()) {
				return;
			}

			missing = missing.OrderBy (x => _Random.Next ()).ToList ();
			var allNodes = _NodeManager.Nodes.OrderBy (x => _Random.Next ()).ToList ();

			foreach (int i in missing) {
				if (allNodes.Any (node => node.RequestPiece (i, this))) {
					_RequestedPiece[i] = true;
					_StepsUntilReadyToRequest = _RequestInterval;
					break;
				}
			}
		}

		public void AddPiece (int index)
		{
			_HasPiece[index] = true;

			if (!IsSeed) {
				if (_HasPiece.All (x => x)) {
					IsSeed = true;

					_ActualR = 255;
					_ActualG = 255;
					_ActualB = 255;
				}
			}
		}

		public bool IsSeed
		{
			get;
			private set;
		}

		public const double CircleRadius = 0.09;

		public void DrawSelf (Graphics graphics, Rectangle clientRectangle)
		{
			var x = _ActualPositionX;
			var y = _ActualPositionY;

			Debug.Assert (x >= 0.0);
			Debug.Assert (y >= 0.0);
			Debug.Assert (x <= 1.0);
			Debug.Assert (y <= 1.0);

			x *= clientRectangle.Width;
			y *= clientRectangle.Height;

			var bufferX = CircleRadius * clientRectangle.Width;
			var bufferY = CircleRadius * clientRectangle.Height;

			var left = x - bufferX;
			var top = y - bufferY;

			RectangleF rectCircle = new RectangleF (
				(float) left,
				(float) top,
				(float) (2.0 * bufferX),
				(float) (2.0 * bufferY));
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.FillEllipse (new SolidBrush (Color), rectCircle);
			//graphics.SmoothingMode = SmoothingMode.None;

			RectangleF rectPieces = new RectangleF (
				(float) (left + bufferX * 0.3),
				(float) (top + bufferY * 0.7),
				(float) (bufferX * 1.4),
				(float) (bufferY * 0.6));
			var availability = new Dictionary<int, float> ();
			for (int i = 0; i < PiecesCount; i++) {
				//availability[i] = HasPiece (i) ? 1 : 0;
				availability[i] = 1f * _HasPartOfPiece[i] / Packet.PartsPerPiece;
			}
			DrawPieceRect (graphics, rectPieces, availability, CompletionPercent, false);
		}

		public static void DrawPieceRect (
			Graphics g,
			RectangleF rect,
			Dictionary<int, float> availability,
			double completionPercent,
			bool framed)
		{
			if (framed) {
				var border = new RectangleF (rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2);
				g.FillRectangle (Brushes.White, border);
			}

			var singleBarWidth = 1f * rect.Width / availability.Count;
			for (int i = 0; i < availability.Count; i++) {
				var r = new RectangleF (
					rect.Left + i * singleBarWidth,
					rect.Top,
					singleBarWidth + 1f, // slight overdraw to prevent glitches
					rect.Height * (1 - availability[i]));
				g.FillRectangle (new SolidBrush (Color.Black), r);

				r.Y += r.Height;
				r.Height = rect.Height * availability[i];
				g.FillRectangle (new SolidBrush (ColorOfPiece (i)), r);
			}

			var font = SystemFonts.DefaultFont;
			var s = completionPercent.ToString ("0") + "%";
			var measure = g.MeasureString (s, font);
			g.DrawString (
				s,
				font,
				Brushes.Gray,
				rect.X + rect.Width / 2 - measure.Width / 2,
				rect.Y + rect.Height / 2 - measure.Height / 2);
		}

		public static Color ColorOfPiece (int index)
		{
			Color color = RgbHslTransform.HSL2RGB ((float) index / (float) PiecesCount, 0.5, 0.5);
			return color;
		}

		public double CompletionPercent
		{
			get
			{
				lock (_HasPiece) {
					return 100.0 * _HasPiece.Count (x => x) / PiecesCount;
				}
			}
		}

		private double _Congestion = 0.0;

		// Return true if piece can be accepted, i.e. when there is no congestion
		public bool TryPrepareReceivePiecePart (int pieceId)
		{
			if (_Congestion > 0) {
				return false;
			}

			_HasPartOfPiece[pieceId]++;
			_Congestion += 0.4;
			return true;
		}
	}
}
