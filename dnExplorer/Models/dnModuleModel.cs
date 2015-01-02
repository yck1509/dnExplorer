using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;

namespace dnExplorer.Models {
	public class dnModuleModel : LazyModel {
		public dnModule Module { get; set; }

		public dnModuleModel(dnModule module) {
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
				yield return new PEImageModel(Module.Image, cor20);

			if (Module.MetaData != null)
				yield return new MetaDataModel(Module.MetaData);

			if (Module.ModuleDef != null)
				yield return new ModuleModel(Module.ModuleDef);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			if (Module.ModuleDef != null && Module.ModuleDef.Assembly != null)
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.assembly.png"), bounds);
			else
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.module.png"), bounds);
		}
	}
}