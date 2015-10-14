using System;
using System.Collections.Generic;
using System.Linq;

namespace BitTorrentVisualization
{
	public class NodeManager
	{
		private readonly Random _Random = new Random ();
		private int _Step;

		public bool IsInDemoMode { get; set; }

		public NodeManager ()
		{
		}

		public void Step ()
		{
			_Step++;

			SetNodePositions ();

			var nodes = Nodes;
			nodes.ForEach (x => x.Step ());

			if (IsInDemoMode) {
				var seeds = nodes.Where (x => x.IsSeed).ToArray ();
				var avail = GlobalAvailability ();
				if (avail.Availability < 1) {
					if (_Random.NextDouble () < 0.01) {
						AddNewNode ().MakeSeed (_Random.NextDouble () < 0.5);
					}
				}

				if (avail.Availability > 2) {
					if (_Random.NextDouble () * 5000 < seeds.Length) {
						RemoveNode (seeds[0]);
					}
				}

				if (_Random.NextDouble () * (nodes.Count - seeds.Count ()) < 0.013) {
					AddNewNode ();
				}
			}

			UpdatePackets ();
		}

		private readonly List<Node> _Nodes = new List<Node> ();

		public List<Node> Nodes
		{
			get
			{
				lock (_Nodes) {
					return new List<Node> (_Nodes);
				}
			}
		}

		public Node AddNewNode ()
		{
			Node node = new Node (this);
			lock (_Nodes) {
				int index = _Random.Next (0, _Nodes.Count + 1);
				_Nodes.Insert (index, node);
			}

			SetNodePositions ();

			return node;
		}

		public void ClearNodes ()
		{
			lock (_Nodes) {
				_Nodes.Clear ();
			}
			lock (_Packets) {
				_Packets.Clear ();
			}
		}

		public void RemoveNode (Node n = null)
		{
			lock (_Nodes) {
				if (n != null) {
					_Nodes.Remove (n);
				}
				else {
					_Nodes.RemoveAt (0);
				}
			}
		}

		public bool DistinctInnerCircle
		{
			get;
			set;
		}

		private void SetNodePositions ()
		{
			const float radiusPeer = 0.4f;
			const float radiusSeed = 0.25f;

			lock (_Nodes) {
				if (DistinctInnerCircle) {
					SetNodePositions (_Nodes.Where (x => x.IsSeed).ToList (), radiusSeed, -0.3f);
					SetNodePositions (_Nodes.Where (x => !x.IsSeed).ToList (), radiusPeer, 1f);
				}
				else {
					SetNodePositions (_Nodes, radiusPeer, 1f);
				}
			}
		}

		private void SetNodePositions (IList<Node> nodes, float radius, float angleFactor)
		{
			double anglePerNode = 2 * Math.PI / nodes.Count ();

			const int StepsPerCircle = 5000;
			double anglePerStep = 2 * Math.PI / StepsPerCircle * angleFactor;

			const int StepsUntilDirectionChange = 1 * StepsPerCircle;
			if ((_Step / StepsUntilDirectionChange % 2) == 0) {
				anglePerStep *= -1;
			}

			for (int i = 0; i < nodes.Count (); i++) {
				double angle = i * anglePerNode + _Step * anglePerStep;

				double x = 0.5f + radius * Math.Cos (angle);
				double y = 0.5f + radius * Math.Sin (angle);

				nodes[i].PositionX = (float) x;
				nodes[i].PositionY = (float) y;

				double h = (double) i / (double) nodes.Count;
				nodes[i].Color = RgbHslTransform.HSL2RGB (h, 1.0, 0.5);
			}
		}

		private readonly List<Packet> _Packets = new List<Packet> ();

		public IEnumerable<Packet> Packets
		{
			get
			{
				lock (_Packets) {
					return _Packets.ToArray ();
				}
			}
		}

		private void UpdatePackets ()
		{
			lock (_Packets) {
				foreach (Packet packet in _Packets.ToArray ()) {
					if (packet.Step ()) {
						_Packets.Remove (packet);
					}
				}
			}
		}

		public void AddPacket (Packet packet)
		{
			lock (_Packets) {
				_Packets.Add (packet);
			}
		}

		public struct GlobalAvailabilityInfo
		{
			public Dictionary<int, float> PieceAvailability;
			public double Availability;
			public double Confidence;
		}

		public GlobalAvailabilityInfo GlobalAvailability ()
		{
			var info = new GlobalAvailabilityInfo ();

			var nodes = Nodes;
			info.PieceAvailability = new Dictionary<int, float> (Node.PiecesCount);
			for (int i = 0; i < Node.PiecesCount; i++) {
				info.PieceAvailability[i] = nodes.Count (x => x.HasPiece (i));
			}

			var availInt = info.PieceAvailability.Min (x => x.Value); // how many clients have all pieces
			var availFrac = 1.0 * info.PieceAvailability.Count (x => x.Value > availInt) / Node.PiecesCount; // how many pieces are superfluous
			info.Availability = availInt + availFrac;

			var confidenceWholeArea = (availFrac * Node.PiecesCount) * (_Nodes.Count () - availInt); // maximum number of superfluous blocks
			var confidenceActualArea = info.PieceAvailability.Sum (x => x.Value - availInt); // actual number superfluous pieces
			info.Confidence = 1.0 * confidenceActualArea / confidenceWholeArea;

			return info;
		}

		public string GetStatistics ()
		{
			var nodes = Nodes;

			var s = "";
			s += nodes.Count () + " node(s) (";
			s += nodes.Count (x => x.IsSeed) + " seed(s))";
			if (nodes.Any ()) {
				s += ", globally " + nodes.Average (x => x.CompletionPercent).ToString ("0") + "% complete";
			}

			var avail = GlobalAvailability ();
			s += ", availability: " + avail.Availability.ToString ("0.00");
			s += " (cfd " + avail.Confidence.ToString ("0.00") + ")";

			s += ", " + Packets.Count () + " packets in transit";

			return s;
		}
	}
}
