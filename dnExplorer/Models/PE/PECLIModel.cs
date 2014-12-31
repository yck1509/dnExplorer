using System;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet.MD;
using dnlib.PE;

namespace dnExplorer.Models {
	public class PECLIModel : DataModel {
		public IPEImage Image { get; set; }
		public ImageCor20Header CLIHeader { get; set; }

		public PECLIModel(IPEImage image, ImageCor20Header header) {
			Image = image;
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