using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class TypeModel : ObjModel, IHasInfo {
		public TypeDef Type { get; set; }

		public override IDnlibDef Definition {
			get { return Type; }
		}

		public TypeModel(TypeDef type) {
			Type = type;
		}

		protected override bool HasChildren {
			get {
				return Type.HasNestedTypes || Type.HasMethods || Type.HasProperties ||
				       Type.HasEvents || Type.HasFields;
			}
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var nestedType in Type.NestedTypes.OrderBy(type => type.Name))
				yield return new TypeModel(nestedType);

			foreach (var method in Type.Methods.OrderBy(method => method.Name)) {
				if (method.SemanticsAttributes != 0)
					continue;
				yield return new MethodModel(method);
			}

			foreach (var property in Type.Properties.OrderBy(property => property.Name))
				yield return new PropertyModel(property);

			foreach (var evnt in Type.Events.OrderBy(evnt => evnt.Name))
				yield return new EventModel(evnt);

			foreach (var field in Type.Fields.OrderBy(field => field.Name))
				yield return new FieldModel(field);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			Image icon, visibility;

			icon = Resources.GetResource<Image>("Icons.ObjModel.type.png");
			if (Type.IsInterface) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.interface.png");
			}
			else if (Type.BaseType != null) {
				if (Type.IsEnum) {
					icon = Resources.GetResource<Image>("Icons.ObjModel.enum.png");
				}
				else if (Type.IsValueType && !Type.IsAbstract) {
					icon = Resources.GetResource<Image>("Icons.ObjModel.valuetype.png");
				}
				else if (Type.IsDelegate()) {
					icon = Resources.GetResource<Image>("Icons.ObjModel.delegate.png");
				}
			}

			switch (Type.Visibility) {
				case TypeAttributes.NotPublic:
				case TypeAttributes.NestedAssembly:
				case TypeAttributes.NestedFamANDAssem:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.internal.png");
					break;
				case TypeAttributes.NestedPrivate:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.private.png");
					break;
				case TypeAttributes.NestedFamily:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.protected.png");
					break;
				case TypeAttributes.NestedFamORAssem:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.famasm.png");
					break;
				case TypeAttributes.Public:
				case TypeAttributes.NestedPublic:
				default:
					visibility = null;
					break;
			}

			g.DrawImageUnscaledAndClipped(icon, bounds);
			if (visibility != null)
				g.DrawImageUnscaledAndClipped(visibility, bounds);
		}

		protected override void Refresh() {
			switch (Type.Visibility) {
				case TypeAttributes.NotPublic:
				case TypeAttributes.NestedAssembly:
				case TypeAttributes.NestedFamANDAssem:
				case TypeAttributes.NestedPrivate:
				case TypeAttributes.NestedFamily:
				case TypeAttributes.NestedFamORAssem:
					ForeColor = Color.Gray;
					break;
				default:
					ForeColor = Color.Empty;
					break;
			}
			Text = Utils.EscapeString(DisplayNameCreator.CreateDisplayName(Type), false);
		}

		string IHasInfo.Header {
			get { return Utils.EscapeString(Type.FullName, false); }
		}

		IEnumerable<KeyValuePair<string, string>> IHasInfo.GetInfos() {
			if (Type.DeclaringType != null)
				yield return
					new KeyValuePair<string, string>("Declaring Type", Utils.EscapeString(Type.DeclaringType.FullName, false));
			if (Type.Scope != null)
				yield return new KeyValuePair<string, string>("Scope", Utils.EscapeString(Type.Scope.ToString(), false));

			yield return new KeyValuePair<string, string>("Token", Type.MDToken.ToStringRaw());
		}
	}
}