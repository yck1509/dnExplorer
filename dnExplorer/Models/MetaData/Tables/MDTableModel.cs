using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet.MD;

namespace dnExplorer.Nodes {
	public class MDTableModel : LazyModel {
		public MDTableHeapModel Parent { get; set; }
		public IMetaData MetaData { get; set; }
		public MDTable MDTable { get; set; }
		public TablesStream Tables { get; set; }

		public MDTableModel(MDTableHeapModel parent, IMetaData metadata, TablesStream stream, MDTable table) {
			Parent = parent;
			MetaData = metadata;
			Tables = stream;
			MDTable = table;
			Text = string.Format("{0:x2}: {1} (0x{2:x})", (byte)table.Table, table.Name, table.Rows);
		}

		protected override bool HasChildren {
			get { return MDTable.Rows > 0; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			for (uint i = 1; i <= MDTable.Rows; i++)
				yield return new MDRowModel(this, i);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.table.png"), bounds);
		}
	}
}