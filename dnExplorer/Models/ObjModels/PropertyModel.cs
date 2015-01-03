using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class PropertyModel : LazyModel, IHasInfo {
		public PropertyDef Property { get; set; }

		public PropertyModel(PropertyDef property) {
			Property = property;
		}

		protected override bool HasChildren {
			get { return Property.GetMethod != null || Property.SetMethod != null || Property.HasOtherMethods; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var child in Property.GetMethods())
				yield return new MethodModel(child);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			Image icon, visibility;

			icon = Resources.GetResource<Image>("Icons.ObjModel.property.png");
			if (Property.SetMethod == null) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.readonly.png");
			}
			else if (Property.GetMethod == null) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.writeonly.png");
			}

			switch (Property.GetVisibility()) {
				case MethodAttributes.CompilerControlled:
				case MethodAttributes.Private:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.private.png");
					break;
				case MethodAttributes.FamANDAssem:
				case MethodAttributes.Assembly:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.internal.png");
					break;
				case MethodAttributes.Family:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.protected.png");
					break;
				case MethodAttributes.FamORAssem:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.famasm.png");
					break;
				case MethodAttributes.Public:
				default:
					visibility = null;
					break;
			}

			g.DrawImageUnscaledAndClipped(icon, bounds);
			if (visibility != null)
				g.DrawImageUnscaledAndClipped(visibility, bounds);
			if (Property.GetMethods().Any(m => m.IsStatic))
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.static.png"), bounds);
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
			get { return Utils.EscapeString(Property.FullName, false); }
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