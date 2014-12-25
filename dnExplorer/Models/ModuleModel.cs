using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet.MD;

namespace dnExplorer.Nodes {
	public class ModuleModel : LazyModel {
		public IMetaData Metadata { get; set; }

		public ModuleModel(IMetaData metadata) {
			Metadata = metadata;
			var nameIndex = metadata.TablesStream.ReadModuleRow(1).Name;
			Text = metadata.StringsStream.Read(nameIndex);
		}

		protected override bool HasChildren {
			get { return true; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			yield return new PEModel(Metadata.PEImage, true);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.module.png"), bounds);
		}
	}
}