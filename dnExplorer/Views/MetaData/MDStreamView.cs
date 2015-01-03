using System;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Models;

namespace dnExplorer.Views {
	public class MDStreamView : ViewBase<MDStreamModel> {
		HexViewer viewer;

		public MDStreamView() {
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
				viewer.Stream = Model.Stream.GetClonedImageStream();
			}
		}

		void OnShowData(object sender, EventArgs e) {
			long begin = (long)Model.Stream.StartOffset;
			long end = (long)Model.Stream.EndOffset;
			if (viewer.HasSelection) {
				end = begin + viewer.SelectionEnd;
				begin += viewer.SelectionStart;
			}
			ViewUtils.ShowRawData(Model, Model.MetaData.PEImage, begin, end);
		}

		public void SelectHexRange(uint begin, uint end) {
			viewer.Select(begin, end);
		}
	}
}