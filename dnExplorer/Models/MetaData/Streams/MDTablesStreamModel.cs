using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet.MD;

namespace dnExplorer.Models {
	public class MDTablesStreamModel : LazyModel {
		public dnModule Module { get; set; }

		public IMetaData MetaData {
			get { return Module.MetaData; }
		}

		public TablesStream Stream { get; set; }

		public MDTablesStreamModel(dnModule module, TablesStream stream) {
			Module = module;
			Stream = stream;
			Text = stream.Name;
		}

		protected override bool HasChildren {
			get { return true; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			yield return new MDTableHeapModel(Module, Stream);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.code.png"), bounds);
		}
	}
}