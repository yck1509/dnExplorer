using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet.MD;

namespace dnExplorer.Models {
	public class MetaDataModel : LazyModel {
		public dnModule Module { get; set; }

		public IMetaData MetaData {
			get { return Module.MetaData; }
		}

		public MetaDataModel(dnModule module) {
			Module = module;
			Text = "Metadata";
		}

		protected override bool HasChildren {
			get { return MetaData.AllStreams.Count > 0; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var stream in MetaData.AllStreams) {
				if (stream is TablesStream)
					yield return new MDTablesStreamModel(Module, (TablesStream)stream);
				else
					yield return new MDStreamModel(MetaData, stream);
			}
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.folder.png"), bounds);
		}
	}
}