using System;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Models;
using dnlib.IO;

namespace dnExplorer.Views {
	public class PESectionView : ViewBase<PESectionModel> {
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
			if (Model == null) {
				viewer.Stream = null;
			}
			else {
				var offset = Model.Section.PointerToRawData;
				var size = Model.Section.SizeOfRawData;
				viewer.Stream = Model.Image.CreateStream((FileOffset)offset, size);
			}
		}

		void OnShowData(object sender, EventArgs e) {
			long offset = Model.Section.PointerToRawData;
			long size = Model.Section.SizeOfRawData;
			if (viewer.HasSelection) {
				offset += viewer.SelectionStart;
				size = viewer.SelectionSize;
			}
			ViewUtils.ShowRawData(Model, Model.Image, offset, offset + size - 1);
		}
	}
}