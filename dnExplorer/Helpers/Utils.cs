using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
				if (c < 0x20 || c >= 0x7f) {
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

		public static uint? ParseInputNum(string input) {
			uint num;
			if (input.StartsWith("0x")) {
				if (!uint.TryParse(input.Substring(2), NumberStyles.HexNumber, null, out num))
					return null;
			}
			else if (input.EndsWith("h")) {
				if (!uint.TryParse(input.Substring(0, input.Length - 1), NumberStyles.HexNumber, null, out num))
					return null;
			}
			else if (input.EndsWith("d")) {
				if (!uint.TryParse(input.Substring(0, input.Length - 1), out num))
					return null;
			}
			else if (!uint.TryParse(input, NumberStyles.HexNumber, null, out num))
				return null;

			return num;
		}

		public static int GetCompressedUInt32Length(uint value) {
			if (value <= 0x7f) {
				return 1;
			}
			if (value <= 0x3fff) {
				return 2;
			}
			if (value > 0x1fffffff) {
				throw new ArgumentOutOfRangeException("UInt32 value can't be compressed");
			}
			return 4;
		}

		public static string ToHexString(this byte[] buffer) {
			return BitConverter.ToString(buffer).Replace("-", "");
		}

		public static string ToHexString(this uint value) {
			return string.Format("0x{0:x8}", value);
		}

		public static void AddListEntry<TKey, TValue>(this IDictionary<TKey, IList<TValue>> self, TKey key, TValue value) {
			IList<TValue> list;
			if (!self.TryGetValue(key, out list))
				self.Add(key, list = new List<TValue>());
			list.Add(value);
		}
	}
}