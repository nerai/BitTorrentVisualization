using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitTorrentVisualization
{
	public class Util
	{
		public static double Dist (double x1, double y1, double x2, double y2)
		{
			return Math.Sqrt ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
		}
	}
}
