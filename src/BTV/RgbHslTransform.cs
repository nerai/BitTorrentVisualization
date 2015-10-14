using System;
using System.Drawing;

namespace BitTorrentVisualization
{
	public static class RgbHslTransform
	{
		// Given H,S,L in range of 0-1
		// Returns a Color (RGB struct) in range of 0-255
		public static Color HSL2RGB (double h, double s, double l)
		{
			// default to gray
			var r = l;
			var g = l;
			var b = l;
			var v = (l <= 0.5) ? (l * (1.0 + s)) : (l + s - l * s);

			if (v > 0) {
				var m = 2 * l - v;
				var sv = (v - m) / v;
				h *= 6.0;
				var sextant = (int) h;
				var fract = h - sextant;
				var vsf = v * sv * fract;
				var mid1 = m + vsf;
				var mid2 = v - vsf;

				switch (sextant) {
					case 0:
						r = v;
						g = mid1;
						b = m;
						break;

					case 1:
						r = mid2;
						g = v;
						b = m;
						break;

					case 2:
						r = m;
						g = v;
						b = mid1;
						break;

					case 3:
						r = m;
						g = mid2;
						b = v;
						break;

					case 4:
						r = mid1;
						g = m;
						b = v;
						break;

					case 5:
						r = v;
						g = m;
						b = mid2;
						break;
				}
			}

			Color rgb = Color.FromArgb (
				Convert.ToByte (r * 255),
				Convert.ToByte (g * 255),
				Convert.ToByte (b * 255));

			return rgb;
		}

		// Given a Color (RGB Struct) in range of 0-255
		// Return H,S,L in range of 0-1
		public static void RGB2HSL (Color rgb, out double h, out double s, out double l)
		{
			var r = rgb.R / 255.0;
			var g = rgb.G / 255.0;
			var b = rgb.B / 255.0;

			h = 0;
			s = 0;
			l = 0;

			var v = Math.Max (Math.Max (r, g), b);
			var m = Math.Min (Math.Min (r, g), b);

			l = (m + v) / 2.0;
			if (l <= 0.0) {
				return;
			}

			var vm = v - m;
			s = vm;
			if (s > 0.0) {
				s /= (l <= 0.5) ? (v + m) : (2.0 - v - m);
			}
			else {
				return;
			}

			var r2 = (v - r) / vm;
			var g2 = (v - g) / vm;
			var b2 = (v - b) / vm;

			if (r == v) {
				h = (g == m ? 5.0 + b2 : 1.0 - g2);
			}
			else if (g == v) {
				h = (b == m ? 1.0 + r2 : 3.0 - b2);
			}
			else {
				h = (r == m ? 3.0 + g2 : 5.0 - r2);
			}

			h /= 6.0;
		}
	}
}
