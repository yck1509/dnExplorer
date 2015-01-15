using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class MethodModel : ObjModel, IHasInfo {
		public MethodDef Method { get; set; }

		public override IDnlibDef Definition {
			get { return Method; }
		}

		public MethodModel(MethodDef method) {
			Method = method;
		}

		protected override bool HasChildren {
			get { return false; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			yield break;
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			ObjectIconRenderer.RenderMethod(Method, g, bounds);
		}

		protected override void Refresh() {
			switch (Method.Access) {
				case MethodAttributes.CompilerControlled:
				case MethodAttributes.Private:
				case MethodAttributes.FamANDAssem:
				case MethodAttributes.Assembly:
				case MethodAttributes.Family:
				case MethodAttributes.FamORAssem:
					ForeColor = Color.Gray;
					break;
				default:
					ForeColor = Color.Empty;
					break;
			}
			Text = Utils.EscapeString(DisplayNameCreator.CreateDisplayName(Method), false);
		}

		string IHasInfo.Header {
			get { return Utils.EscapeString(Method.FullName, false); }
		}

		IEnumerable<KeyValuePair<string, string>> IHasInfo.GetInfos() {
			yield return
				new KeyValuePair<string, string>("Declaring Type", Utils.EscapeString(Method.DeclaringType.FullName, false));
			if (Method.DeclaringType.Scope != null)
				yield return
					new KeyValuePair<string, string>("Scope", Utils.EscapeString(Method.DeclaringType.Scope.ToString(), false));

			yield return new KeyValuePair<string, string>("Token", Method.MDToken.ToStringRaw());
			if (Method.RVA != 0) {
				yield return new KeyValuePair<string, string>("RVA", ((uint)Method.RVA).ToHexString());
				var fileOffset = ((ModuleDefMD)Method.Module).MetaData.PEImage.ToFileOffset(Method.RVA);
				yield return new KeyValuePair<string, string>("File Offset", ((uint)fileOffset).ToHexString());

				if (Method.HasBody) {
					if (Method.Body.LocalVarSigTok != 0)
						yield return
							new KeyValuePair<string, string>("LocalVarSigTok", new MDToken(Method.Body.LocalVarSigTok).ToStringRaw());
				}
			}
		}
	}
}