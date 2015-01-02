using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class ModuleModel : LazyModel {
		public ModuleDefMD Module { get; set; }

		public ModuleModel(ModuleDefMD module) {
			Module = module;
			Text = module.Name ?? "<<<NULL>>>";
		}

		protected override bool HasChildren {
			get { return true; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var pair in Module.Types.GroupBy(type => type.Namespace).OrderBy(gp => gp.Key))
				yield return new NamespaceModel(pair.Key, pair.ToArray());
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.module.png"), bounds);
		}
	}
}