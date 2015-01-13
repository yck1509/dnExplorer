using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Models;

namespace dnExplorer.Views {
	public class MDTablesStreamView : ViewBase<MDTablesStreamModel> {
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

			hexView.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			var nav = new ToolStripMenuItem("Show in Raw Data");
			nav.Click += OnShowData;
			hexView.ContextMenuStrip.Items.Add(nav);
		}

		protected override void OnModelUpdated() {
			gridView.Clear();
			if (Model != null) {
				hexView.ClearHighLight();

				var tbls = Model.Stream;
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
				var numTbls = Math.Min(Utils.GetNumOfSetBits(tbls.ValidMask), 0x3f);
				var rowSizeEnd = rowSizeBegin + numTbls * 4;
				hexView.AddHighLight(new HexViewer.HighLight(Color.Green, rowSizeBegin, rowSizeEnd));

				if (tbls.HasExtraData) {
					gridView.AddRow("ExtraData (!)", ranges.IncrementOffset(relBase, ref rowSizeEnd, 4),
						tbls.ExtraData);
				}

				hexView.AddHighLights(ranges, Color.Red, Color.Blue);

				hexView.Stream = tbls.GetClonedImageStream();
			}
			else
				hexView.Stream = null;
		}

		void OnShowData(object sender, EventArgs e) {
			long begin = (long)Model.Stream.StartOffset;
			long end = (long)Model.Stream.EndOffset;
			if (hexView.HasSelection) {
				end = begin + hexView.SelectionEnd;
				begin += hexView.SelectionStart;
			}
			ViewUtils.ShowRawData(App, Model, Model.MetaData.PEImage, begin, end);
		}

		public void SelectHexRange(uint begin, uint end) {
			hexView.Select(begin, end);
		}
	}
}