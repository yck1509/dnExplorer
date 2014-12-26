using System;
using dnExplorer.Controls;
using dnExplorer.Nodes;
using dnlib.IO;

namespace dnExplorer.Views {
	public class PESectionView : ViewBase {
		HexViewer viewer;

		public PESectionView() {
			viewer = new HexViewer();
			Controls.Add(viewer);
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
	}
}