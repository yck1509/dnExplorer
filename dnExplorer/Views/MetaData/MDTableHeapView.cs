using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Nodes;
using dnExplorer.Trees;
using dnlib.DotNet;
using dnlib.DotNet.MD;

namespace dnExplorer.Views {
	public class MDTableHeapView : ViewBase {
		TreeViewX treeView;
		GridView gridView;
		HexViewer hexView;
		Dictionary<Table, HexViewer.HighLight[]> hls;

		public MDTableHeapView() {
			var split1 = new SplitContainer {
				Orientation = Orientation.Vertical,
				Dock = DockStyle.Fill
			};
			Controls.Add(split1);

			treeView = new TreeViewX {
				Dock = DockStyle.Fill
			};
			treeView.AfterSelect += OnNodeSelected;
			split1.Panel1.Controls.Add(treeView);

			var split2 = new SplitContainer {
				Orientation = Orientation.Horizontal,
				Dock = DockStyle.Fill
			};
			split1.Panel2.Controls.Add(split2);

			gridView = new GridView();
			gridView.AddColumn(new GridView.Column("Field", true, 110));
			gridView.AddColumn(new GridView.Column("Type", true));
			gridView.AddColumn(new GridView.Column("Offset", true));
			gridView.AddColumn(new GridView.Column("Value", false));
			gridView.AddColumn(new GridView.Column("Description", false, 250));
			split2.Panel1.Controls.Add(gridView);

			hexView = new HexViewer();
			split2.Panel2.Controls.Add(hexView);

			PerformLayout();

			hls = new Dictionary<Table, HexViewer.HighLight[]>();

			hexView.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			var nav = new ToolStripMenuItem("Show in Raw Data");
			nav.Click += OnShowData;
			hexView.ContextMenuStrip.Items.Add(nav);
		}

		public void SelectItem(MDToken token) {
			var table = ((MDTableHeapModel)Model).Stream.Get(token.Table);
			var rid = token.Rid;
			if (table == null || !table.IsValidRID(rid)) {
				MessageBox.Show("Invalid token.", Main.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			TreeNavigator.Create()
				.Path<MDTableModel>(m => m.Table == table ? NavigationState.In : NavigationState.Next)
				.Path<MDRowModel>(m => m.Rid == rid ? NavigationState.Done : NavigationState.Next)
				.Navigate(treeView);
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

		uint CalculateTableRowsOffset(Table table) {
			var tbls = ((MDTableHeapModel)Model).Stream;
			uint offset = 24;

			var valid = tbls.ValidMask;
			for (int i = 0; i < 0x40; i++) {
				if ((Table)i == table)
					break;

				if ((valid & 1) == 1)
					offset += 4;
				valid = valid >> 1;
			}
			return offset;
		}

		HexViewer.HighLight[] GetHighLights(Table table) {
			HexViewer.HighLight[] ret;
			if (hls.TryGetValue(table, out ret))
				return ret;

			var tbls = ((MDTableHeapModel)Model).Stream;
			var mdTable = tbls.Get(table);
			ret = new HexViewer.HighLight[mdTable.Rows + 1];

			var rowCountOffset = CalculateTableRowsOffset(table);
			var rowOffset = (uint)mdTable.StartOffset - (uint)tbls.StartOffset;

			using (var colorSeq = Utils.GetAlternateColorSequence(Color.Green, Color.Orange).GetEnumerator()) {
				ret[0] = new HexViewer.HighLight(Color.Brown, rowCountOffset, rowCountOffset + 4);
				for (int i = 1; i < ret.Length && colorSeq.MoveNext(); i++) {
					ret[i] = new HexViewer.HighLight(colorSeq.Current, rowOffset, rowOffset + mdTable.RowSize);
					rowOffset += mdTable.RowSize;
				}
			}
			hls[table] = ret;
			return ret;
		}

		void OnNodeSelected(object sender, TreeViewEventArgs e) {
			gridView.Clear();

			var node = (DataTreeNodeX)treeView.SelectedNode;
			if (node.Model is MDTableModel) {
				var mdTable = ((MDTableModel)node.Model).Table;
				hexView.SetHighLights(GetHighLights(mdTable.Table));
			}
			else if (node.Model is MDRowModel) {
				var rowModel = (MDRowModel)node.Model;
				var mdTable = rowModel.MDTable;
				var rid = rowModel.Rid;

				var rowHL = GetHighLights(mdTable.Table);
				rowHL = (HexViewer.HighLight[])rowHL.Clone();

				var row = rowHL[rid];
				row = new HexViewer.HighLight(Color.Blue, row.Start, row.End);
				rowHL[rid] = row;

				hexView.SetHighLights(rowHL);
				RowInfoPopulator.PopulateGridView(gridView, rowModel);
			}
			else {
				hexView.ClearHighLight();
			}
		}

		protected override void OnModelUpdated() {
			var model = (MDTableHeapModel)Model;

			treeView.BeginUpdate();
			treeView.Nodes.Clear();
			hls.Clear();

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

		void OnShowData(object sender, EventArgs e) {
			var model = (MDTableHeapModel)Model;

			long begin = (long)model.Stream.StartOffset;
			long end = (long)model.Stream.EndOffset;
			if (hexView.HasSelection) {
				end = begin + hexView.SelectionEnd;
				begin += hexView.SelectionStart;
			}
			ViewUtils.ShowRawData(Model, model.MetaData.PEImage, begin, end);
		}
	}
}