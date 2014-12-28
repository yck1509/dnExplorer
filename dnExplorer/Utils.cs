using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using dnExplorer.Controls;

namespace dnExplorer {
	public static class Utils {
		public static IEnumerable<Color> GetAlternateColorSequence(Color colorA, Color colorB) {
			var currentColor = Color.Empty;
			while (true) {
				currentColor = (currentColor == colorA ? colorB : colorA);
				yield return currentColor;
			}
		}

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


		public static string EscapeString(string s, bool addQuotes) {
			var sb = new StringBuilder();
			if (s == null) {
				sb.Append("null");
				return sb.ToString();
			}

			if (addQuotes)
				sb.Append('\'');

			foreach (var c in s) {
				if (c < 0x20) {
					switch (c) {
						case '\a':
							sb.Append(@"\a");
							break;
						case '\b':
							sb.Append(@"\b");
							break;
						case '\f':
							sb.Append(@"\f");
							break;
						case '\n':
							sb.Append(@"\n");
							break;
						case '\r':
							sb.Append(@"\r");
							break;
						case '\t':
							sb.Append(@"\t");
							break;
						case '\v':
							sb.Append(@"\v");
							break;
						default:
							sb.Append(string.Format(@"\u{0:X4}", (int)c));
							break;
					}
				}
				else if (c == '\\' || c == '\'' || c == '\"') {
					sb.Append('\\');
					sb.Append(c);
				}
				else
					sb.Append(c);
			}

			if (addQuotes)
				sb.Append('\'');
			return sb.ToString();
		}
	}
}