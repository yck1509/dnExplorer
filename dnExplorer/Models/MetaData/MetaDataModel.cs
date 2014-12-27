using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet.MD;

namespace dnExplorer.Nodes {
	public class MetaDataModel : LazyModel {
		public IMetaData MetaData { get; set; }

		public MetaDataModel(IMetaData metadata) {
			MetaData = metadata;
			Text = "Metadata";
		}

		protected override bool HasChildren {
			get { return MetaData.AllStreams.Count > 0; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			yield break;
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.folder.png"), bounds);
		}
	}
}