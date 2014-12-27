using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet.MD;

namespace dnExplorer.Nodes {
	public class MDTableModel : LazyModel {
		public IMetaData MetaData { get; set; }
		public MDTable Table { get; set; }
		public TablesStream Stream { get; set; }

		public MDTableModel(IMetaData metadata, TablesStream stream, MDTable table) {
			MetaData = metadata;
			Stream = stream;
			Table = table;
			Text = string.Format("{0} (0x{1:x})", table.Name, table.Rows);
		}

		protected override bool HasChildren {
			get { return Table.Rows > 0; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			for (uint i = 1; i <= Table.Rows; i++)
				yield return new MDRowModel(MetaData, Stream, Table, i);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.table.png"), bounds);
		}
	}
}