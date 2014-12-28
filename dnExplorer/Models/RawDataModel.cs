using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;

namespace dnExplorer.Nodes {
	public class RawDataModel : LazyModel {
		public byte[] Data { get; set; }

		public RawDataModel(byte[] data) {
			Data = data;
			Text = "Raw Data";
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