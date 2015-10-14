using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace BitTorrentVisualization
{
	public class Packet
	{
		private static readonly Random _R = new Random ();

		private readonly Node _Sender;
		private readonly Node _Receiver;
		private readonly int _PieceId;

		private PacketTail _Tail;

		public const int PartsPerPiece = 50;

		public Packet (Node sender, Node receiver, int pieceId)
		{
			_Sender = sender;
			_Receiver = receiver;
			_PieceId = pieceId;

			var x = _Sender.PositionX;
			var y = _Sender.PositionY;

			for (int i = 0; i < PartsPerPiece; i++) {
				_Tail = new PacketTail (
					this,
					0.3 + 0.3 * _R.NextDouble (),
					_Tail);
			}
		}

		/// <returns>
		/// True if the packed arrived
		/// </returns>
		public bool Step ()
		{
			if (_Tail == null) {
				return true;
			}
			if (_Tail.Step (_Receiver.PositionX, _Receiver.PositionY)) {
				_Tail = _Tail._Next;
			}
			if (_Tail == null) {
				_Receiver.AddPiece (_PieceId);
				return true;
			}
			return false;
		}

		public void DrawSelf (Graphics g, Rectangle clientRect)
		{
			if (_Tail != null) {
				_Tail.DrawTail (g, clientRect);
			}
		}

		private class PacketTail
		{
			private static readonly Random _R = new Random ();

			private readonly Packet _Packet;

			public PacketTail _Next;

			private double _X;
			private double _Y;
			private double _DX;
			private double _DY;

			private double _Size;
			private int _Delay;
			private bool _HasEnteredTarget;

			public PacketTail (Packet packet, double size, PacketTail next)
			{
				_Packet = packet;
				_Size = size;
				_Next = next;
				_Delay = _R.Next (1, 4);
			}

			// Return true if arrived
			public bool Step (float tx, float ty)
			{
				if (_Delay > 0) {
					_Delay--;
					if (_Delay == 0) {
						_X = _Packet._Sender.PositionX;
						_Y = _Packet._Sender.PositionY;
					}
					return false;
				}

				if (_Next != null) {
					_Next.Step (_Packet._Receiver.PositionX, _Packet._Receiver.PositionY);
				}

				const float movementPerStep = 0.005f;
				var d = Util.Dist (_X, _Y, tx, ty);
				_DX = _Packet._Receiver.PositionX;
				_DY = _Packet._Receiver.PositionY;

				if (_HasEnteredTarget) {
					if (d < movementPerStep) {
						return true;
					}
				}
				else {
					if (d <= Node.CircleRadius + 0.01) {
						if (_Packet._Receiver.TryPrepareReceivePiecePart (_Packet._PieceId)) {
							_HasEnteredTarget = true;
						}
						else {
							_DX = _R.NextDouble ();
							_DY = _R.NextDouble ();
						}
					}
				}

				_DX -= _X;
				_DY -= _Y;
				var scale = movementPerStep / (float) Math.Sqrt (_DX * _DX + _DY * _DY);
				_DX *= scale;
				_DY *= scale;
				_X += _DX;
				_Y += _DY;

				return false;
			}

			public void DrawTail (Graphics g, Rectangle clientRect)
			{
				if (_Delay > 0) {
					return;
				}

				var x = _X;
				var y = _Y;
				x *= clientRect.Width;
				y *= clientRect.Height;

				const double CircleRadius = 0.007;
				var bufferX = CircleRadius * _Size * clientRect.Width;
				var bufferY = CircleRadius * _Size * clientRect.Height;
				var left = x - bufferX;
				var top = y - bufferY;

				var rectCircle = new RectangleF ((float) left, (float) top, (float) (2.0 * bufferX), (float) (2.0 * bufferY));
				g.SmoothingMode = SmoothingMode.AntiAlias;
				using (var brush = new SolidBrush (Node.ColorOfPiece (_Packet._PieceId))) {
					g.FillEllipse (brush, rectCircle);
				}
				g.SmoothingMode = SmoothingMode.None;

				if (_Next != null) {
					_Next.DrawTail (g, clientRect);
				}
			}
		}
	}
}
