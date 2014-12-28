using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;

namespace dnExplorer.Nodes {
	public class ModuleModel : LazyModel {
		public dnModule Module { get; set; }

		public ModuleModel(dnModule module) {
			Module = module;
			Text = module.Name;
		}

		protected override bool HasChildren {
			get { return true; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			var cor20 = Module.MetaData == null ? null : Module.MetaData.ImageCor20Header;

			yield return new RawDataModel(Module.RawData);

			if (Module.Image != null)
				yield return new PEImageModel(Module.MetaData.PEImage, cor20);

			if (Module.MetaData != null)
				yield return new MetaDataModel(Module.MetaData);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.module.png"), bounds);
		}
	}
}