using System;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Nodes;
using dnExplorer.Trees;
using dnlib.DotNet.MD;

namespace dnExplorer.Views {
	public class MDTableHeapView : ViewBase {
		TreeViewX treeView;
		GridView gridView;
		HexViewer hexView;

		public MDTableHeapView() {
			var split1 = new SplitContainer {
				Orientation = Orientation.Vertical,
				Dock = DockStyle.Fill
			};
			Controls.Add(split1);

			treeView = new TreeViewX {
				Dock = DockStyle.Fill
			};
			split1.Panel1.Controls.Add(treeView);

			var split2 = new SplitContainer {
				Orientation = Orientation.Horizontal,
				Dock = DockStyle.Fill
			};
			split1.Panel2.Controls.Add(split2);

			gridView = new GridView();
			split2.Panel1.Controls.Add(gridView);

			hexView = new HexViewer();
			split2.Panel2.Controls.Add(hexView);

			PerformLayout();
		}

		void UpdateTreeView(IMetaData metadata, TablesStream stream) {
			for (int i = 0; i < 0x40; i++) {
				var table = (Table)i;
				if (stream.HasTable(table)) {
					var mdTable = stream.Get(table);
					if (mdTable.Rows > 0)
						treeView.Nodes.Add(new MDTableModel(metadata, stream, mdTable).ToNode());
				}
			}
		}

		protected override void OnModelUpdated() {
			var model = (MDTableHeapModel)Model;

			treeView.BeginUpdate();
			treeView.Nodes.Clear();

			gridView.Clear();
			if (model != null) {
				UpdateTreeView(model.MetaData, model.Stream);

				hexView.ClearHighLight();
				hexView.Stream = model.Stream.GetClonedImageStream();
			}
			else
				hexView.Stream = null;

			treeView.EndUpdate();
		}
	}
}