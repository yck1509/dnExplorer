using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.PE;

namespace dnExplorer.Models {
	public class PESectionsModel : LazyModel {
		public IPEImage Image { get; set; }

		public PESectionsModel(IPEImage peImage) {
			Image = peImage;
			Text = "Sections";
		}

		protected override bool HasChildren {
			get { return Image.ImageSectionHeaders.Count > 0; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var section in Image.ImageSectionHeaders)
				yield return new PESectionModel(Image, section);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.folder.png"), bounds);
		}
	}
}