using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using dnExplorer.Trees;
using dnlib.PE;

namespace dnExplorer.Nodes {
	public class PEImageModel : LazyModel {
		public IPEImage Image { get; set; }

		public PEImageModel(IPEImage peImage, bool metadataParent) {
			Image = peImage;
			if (metadataParent)
				Text = "PE Image";
			else
				Text = Path.GetFileName(peImage.FileName);
		}

		protected override bool HasChildren {
			get { return Image.ImageSectionHeaders.Count > 0; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			yield return new PESectionsModel(Image);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.folder.png"), bounds);
		}
	}
}