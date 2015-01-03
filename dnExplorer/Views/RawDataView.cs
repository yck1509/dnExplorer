using System;
using dnExplorer.Controls;
using dnExplorer.Models;
using dnlib.IO;

namespace dnExplorer.Views {
	public class RawDataView : ViewBase<RawDataModel> {
		HexViewer viewer;

		public RawDataView() {
			viewer = new HexViewer();
			Controls.Add(viewer);
		}

		public void Select(long begin, long end) {
			viewer.Select(begin, end);
		}

		protected override void OnModelUpdated() {
			if (Model == null) {
				viewer.Stream = null;
			}
			else {
				viewer.Stream = new MemoryImageStream(0, Model.Data, 0, Model.Data.Length);
			}
		}
	}
}