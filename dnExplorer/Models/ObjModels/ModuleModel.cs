using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class ModuleModel : ObjModel, IHasInfo {
		public ModuleDefMD Module { get; set; }

		public override IDnlibDef Definition {
			get { return Module; }
		}

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

		string IHasInfo.Header {
			get { return Utils.EscapeString(Module.Name, false); }
		}

		IEnumerable<KeyValuePair<string, string>> IHasInfo.GetInfos() {
			yield return new KeyValuePair<string, string>("Version", (Module.Mvid ?? Guid.Empty).ToString("D"));
			yield return new KeyValuePair<string, string>("Kind", Module.Kind.ToString());
			yield return new KeyValuePair<string, string>("Flags", Module.Cor20HeaderFlags.ToString());
			yield return new KeyValuePair<string, string>("Machine", Module.Machine.ToString());
			yield return new KeyValuePair<string, string>("Runtime", Utils.EscapeString(Module.RuntimeVersion, false));
			yield return new KeyValuePair<string, string>("Token", Module.MDToken.ToStringRaw());
		}
	}
}