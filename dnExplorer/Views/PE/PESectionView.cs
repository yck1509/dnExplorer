using System;
using dnExplorer.Nodes;

namespace dnExplorer.Views {
	public class PESectionView : ViewBase {
		protected override void OnModelUpdated() {
			var model = (PESectionModel)Model;
			if (model == null) {
			}
			else {
				var offset = model.Section.PointerToRawData;
				var size = model.Section.SizeOfRawData;
			}
		}
	}
}