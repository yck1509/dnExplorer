using System;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet.MD;

namespace dnExplorer.Nodes {
	public class PECLIModel : DataModel {
		public ImageCor20Header CLIHeader { get; set; }

		public PECLIModel(ImageCor20Header header) {
			CLIHeader = header;
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.code.png"), bounds);
		}

		public override void Refresh(bool refreshChildren) {
			Text = "CLI Header";
		}
	}
}