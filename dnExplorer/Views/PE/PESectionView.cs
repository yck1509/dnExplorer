using System;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Nodes;
using dnlib.IO;

namespace dnExplorer.Views {
	public class PESectionView : ViewBase {
		HexViewer viewer;

		public PESectionView() {
			viewer = new HexViewer();
			Controls.Add(viewer);

			viewer.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			var nav = new ToolStripMenuItem("Show in Raw Data");
			nav.Click += OnShowData;
			viewer.ContextMenuStrip.Items.Add(nav);
		}

		protected override void OnModelUpdated() {
			var model = (PESectionModel)Model;
			if (model == null) {
				viewer.Stream = null;
			}
			else {
				var offset = model.Section.PointerToRawData;
				var size = model.Section.SizeOfRawData;
				viewer.Stream = model.Image.CreateStream((FileOffset)offset, size);
			}
		}

		void OnShowData(object sender, EventArgs e) {
			var model = (PESectionModel)Model;

			long offset = model.Section.PointerToRawData;
			long size = model.Section.SizeOfRawData;
			if (viewer.HasSelection) {
				offset += viewer.SelectionStart;
				size = viewer.SelectionSize;
			}
			ViewUtils.ShowRawData(Model, model.Image, offset, offset + size - 1);
		}
	}
}