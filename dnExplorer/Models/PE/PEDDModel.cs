using System;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.PE;

namespace dnExplorer.Nodes {
	public class PEDDModel : DataModel {
		public IPEImage Image { get; set; }

		public PEDDModel(IPEImage peImage) {
			Image = peImage;
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.code.png"), bounds);
		}

		public override void Refresh(bool refreshChildren) {
			Text = "Data Directories";
		}
	}
}