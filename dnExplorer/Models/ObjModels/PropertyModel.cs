using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class PropertyModel : ObjModel, IHasInfo {
		public PropertyDef Property { get; set; }

		public override IDnlibDef Definition {
			get { return Property; }
		}

		public PropertyModel(PropertyDef property) {
			Property = property;
		}

		protected override bool HasChildren {
			get { return !Property.IsEmpty; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var child in Property.GetAccessors())
				yield return new MethodModel(child);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			ObjectIconRenderer.RenderProperty(Property, g, bounds);
		}

		protected override void Refresh() {
			switch (Property.GetVisibility()) {
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
			Text = Utils.EscapeString(DisplayNameCreator.CreateDisplayName(Property), false);
		}

		string IHasInfo.Header {
			get { return Utils.EscapeString(DisplayNameCreator.CreateFullName(Property), false); }
		}

		IEnumerable<KeyValuePair<string, string>> IHasInfo.GetInfos() {
			yield return
				new KeyValuePair<string, string>("Declaring Type", Utils.EscapeString(Property.DeclaringType.FullName, false));
			if (Property.DeclaringType.Scope != null)
				yield return
					new KeyValuePair<string, string>("Scope", Utils.EscapeString(Property.DeclaringType.Scope.ToString(), false));

			yield return new KeyValuePair<string, string>("Token", Property.MDToken.ToStringRaw());
			yield return
				new KeyValuePair<string, string>("Property Type", Utils.EscapeString(Property.PropertySig.RetType.FullName, false));
		}
	}
}