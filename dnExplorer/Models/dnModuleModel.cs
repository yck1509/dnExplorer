using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;

namespace dnExplorer.Models {
	public class dnModuleModel : LazyModel, IHasInfo {
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

		string IHasInfo.Header {
			get {
				if (Module.ModuleDef != null && Module.ModuleDef.Assembly != null)
					return Module.ModuleDef.Assembly.FullName;
				return Module.Name;
			}
		}

		IEnumerable<KeyValuePair<string, string>> IHasInfo.GetInfos() {
			if (Module.Image != null)
				yield return new KeyValuePair<string, string>("Location", Module.Image.FileName);
			yield return new KeyValuePair<string, string>("Size", Module.RawData.Length + " Bytes");
			if (Module.ModuleDef != null && Module.ModuleDef.Assembly != null) {
				var asmDef = Module.ModuleDef.Assembly;
				if (!asmDef.PublicKey.IsNullOrEmpty)
					yield return new KeyValuePair<string, string>("Public Key", asmDef.PublicKey.Data.ToHexString());
				if (asmDef.PublicKey.Token != null && !asmDef.PublicKey.Token.IsNullOrEmpty)
					yield return new KeyValuePair<string, string>("Public Key Token", asmDef.PublicKey.Token.Data.ToHexString());
				yield return new KeyValuePair<string, string>("Token", asmDef.MDToken.ToStringRaw());
			}
		}
	}
}