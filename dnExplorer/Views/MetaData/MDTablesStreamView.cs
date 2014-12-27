using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Nodes;

namespace dnExplorer.Views {
	public class MDTablesStreamView : ViewBase {
		GridView gridView;
		HexViewer hexView;

		public MDTablesStreamView() {
			var split = new SplitContainer {
				Orientation = Orientation.Horizontal,
				Dock = DockStyle.Fill
			};
			Controls.Add(split);

			gridView = new GridView();
			gridView.AddColumn(new GridView.Column("Field", true, 120));
			gridView.AddColumn(new GridView.Column("Offset", true));
			gridView.AddColumn(new GridView.Column("Value", false, 150));
			split.Panel1.Controls.Add(gridView);

			hexView = new HexViewer();
			split.Panel2.Controls.Add(hexView);

			PerformLayout();
		}

		protected override void OnModelUpdated() {
			var model = (MDTablesStreamModel)Model;
			gridView.Clear();
			if (model != null) {
				hexView.ClearHighLight();

				var tbls = model.Stream;
				var ranges = new List<Tuple<uint, uint>>();
				uint relBase = (uint)tbls.StartOffset;
				uint offset = 0;

				gridView.AddRow("Reserved1", ranges.IncrementOffset(relBase, ref offset, 4),
					tbls.Reserved1);

				gridView.AddRow("MajorVersion", ranges.IncrementOffset(relBase, ref offset, 1),
					(byte)(tbls.Version >> 8));

				gridView.AddRow("MinorVersion", ranges.IncrementOffset(relBase, ref offset, 1),
					(byte)tbls.Version);

				gridView.AddRow("Flags", ranges.IncrementOffset(relBase, ref offset, 1),
					tbls.Flags);

				gridView.AddRow("Log2Rid", ranges.IncrementOffset(relBase, ref offset, 1),
					tbls.Log2Rid);

				gridView.AddRow("MaskValid", ranges.IncrementOffset(relBase, ref offset, 8),
					tbls.ValidMask);

				gridView.AddRow("MaskSorted", ranges.IncrementOffset(relBase, ref offset, 8),
					tbls.SortedMask);

				var rowSizeBegin = offset;
				var numTbls = Math.Min(Utils.GetNumOfSetBits(tbls.ValidMask), 0x40);
				var rowSizeEnd = rowSizeBegin + numTbls * 4;
				hexView.AddHighLight(new HexViewer.HighLight(Color.Green, rowSizeBegin, rowSizeEnd));

				if (tbls.HasExtraData) {
					gridView.AddRow("ExtraData (!)", ranges.IncrementOffset(relBase, ref offset, 4),
						tbls.ExtraData);
				}

				hexView.AddHighLights(ranges, Color.Red, Color.Blue);

				hexView.Stream = tbls.GetClonedImageStream();
			}
			else
				hexView.Stream = null;
		}
	}
}