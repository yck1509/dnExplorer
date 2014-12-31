using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet.MD;

namespace dnExplorer.Models {
	public class MDStreamModel : LazyModel {
		public IMetaData MetaData { get; set; }
		public DotNetStream Stream { get; set; }

		public MDStreamModel(IMetaData metadata, DotNetStream stream) {
			MetaData = metadata;
			Stream = stream;
			Text = stream.Name;
		}

		protected override bool HasChildren {
			get { return false; }
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
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.code.png"), bounds);
		}
	}
}