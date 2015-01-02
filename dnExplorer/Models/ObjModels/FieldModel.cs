using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class FieldModel : LazyModel {
		public FieldDef Field { get; set; }

		public FieldModel(FieldDef field) {
			Field = field;
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
			Image visibility;

			switch (Field.Access) {
				case FieldAttributes.CompilerControlled:
				case FieldAttributes.Private:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.private.png");
					break;
				case FieldAttributes.FamANDAssem:
				case FieldAttributes.Assembly:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.internal.png");
					break;
				case FieldAttributes.Family:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.protected.png");
					break;
				case FieldAttributes.FamORAssem:
					visibility = Resources.GetResource<Image>("Icons.ObjModel.famasm.png");
					break;
				case FieldAttributes.Public:
				default:
					visibility = null;
					break;
			}

			if (Field.IsStatic) {
				if (Field.DeclaringType.IsEnum)
					g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.constant.png"), bounds);
				else {
					g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.field.png"), bounds);
					if (visibility != null)
						g.DrawImageUnscaledAndClipped(visibility, bounds);
					g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.static.png"), bounds);
				}
			}
			else {
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.field.png"), bounds);
				if (visibility != null)
					g.DrawImageUnscaledAndClipped(visibility, bounds);
			}
		}

		protected override void Refresh() {
			switch (Field.Access) {
				case FieldAttributes.CompilerControlled:
				case FieldAttributes.Private:
				case FieldAttributes.FamANDAssem:
				case FieldAttributes.Assembly:
				case FieldAttributes.Family:
				case FieldAttributes.FamORAssem:
					ForeColor = Color.Gray;
					break;
				default:
					ForeColor = Color.Empty;
					break;
			}
			Text = Utils.EscapeString(DisplayNameCreator.CreateDisplayName(Field), false);
		}
	}
}