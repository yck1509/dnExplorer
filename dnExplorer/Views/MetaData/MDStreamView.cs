using System;
using dnExplorer.Controls;
using dnExplorer.Nodes;

namespace dnExplorer.Views {
	public class MDStreamView : ViewBase {
		HexViewer viewer;

		public MDStreamView() {
			viewer = new HexViewer();
			Controls.Add(viewer);
		}

		protected override void OnModelUpdated() {
			var model = (MDStreamModel)Model;
			if (model == null) {
				viewer.Stream = null;
			}
			else {
				viewer.Stream = model.Stream.GetClonedImageStream();
			}
		}
	}
}