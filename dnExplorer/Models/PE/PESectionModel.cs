using System;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.PE;

namespace dnExplorer.Models {
	public class PESectionModel : DataModel {
		public IPEImage Image { get; set; }
		public ImageSectionHeader Section { get; set; }

		public PESectionModel(IPEImage peImage, ImageSectionHeader section) {
			Image = peImage;
			Section = section;
			Text = section.DisplayName;
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.code.png"), bounds);
		}

		public override void Refresh(bool refreshChildren) {
			Text = Section.DisplayName;
		}
	}
}