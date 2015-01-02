using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class NamespaceModel : LazyModel {
		public UTF8String Namespace { get; set; }
		public TypeDef[] Types { get; set; }

		public NamespaceModel(UTF8String nameSpace, TypeDef[] types) {
			Namespace = nameSpace;
			Types = types;
			Text = Utils.EscapeString(Namespace.String ?? "<<<NULL>>>", false);
			if (Text == "") Text = "-";
		}

		protected override bool HasChildren {
			get { return true; }
		}

		protected override bool IsVolatile {
			get { return true; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var type in Types.OrderBy(type => type.Name))
				yield return new TypeModel(type);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.namespace.png"), bounds);
		}
	}
}