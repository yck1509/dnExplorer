using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class MethodModel : LazyModel {
		public MethodDef Method { get; set; }

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
			Image icon, visibility;

			icon = Resources.GetResource<Image>("Icons.ObjModel.method.png");
			if (Method.IsConstructor) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.constructor.png");
			}
			else if (Method.IsVirtual && !Method.IsAbstract) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.override.png");
			}

			switch (Method.Access) {
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
			if (Method.IsStatic)
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.static.png"), bounds);
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
	}
}