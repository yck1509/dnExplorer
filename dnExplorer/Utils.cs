using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Controls;

namespace dnExplorer {
	public static class Utils {
		public static void AddHighLights(this HexViewer hexView, List<Tuple<uint, uint>> ranges, Color colorA, Color colorB) {
			var currentColor = Color.Empty;
			foreach (var range in ranges) {
				currentColor = (currentColor == colorA ? colorB : colorA);
				hexView.AddHighLight(new HexViewer.HighLight(currentColor, range.Item1, range.Item2));
			}
		}

		public static uint IncrementOffset(this List<Tuple<uint, uint>> ranges, uint relBase, ref uint offset, uint increment) {
			ranges.Add(Tuple.Create(offset, offset + increment));
			uint ret = offset + relBase;
			offset += increment;
			return ret;
		}

		public static uint GetNumOfSetBits(ulong value) {
			uint bits = 0;
			while (value != 0) {
				if ((value & 1) != 0)
					bits++;
				value >>= 1;
			}
			return bits;
		}
	}
}