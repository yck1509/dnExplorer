using System;
using System.Drawing;
using System.Linq;
using dnlib.DotNet;

namespace dnExplorer {
	public static class ObjectIconRenderer {
		public static void Render(object obj, Graphics g, Rectangle bounds) {
			if (obj is IAssembly)
				RenderAssembly((IAssembly)obj, g, bounds);
			else if (obj is IModule)
				RenderModule((IModule)obj, g, bounds);
			else if (obj is ITypeDefOrRef)
				RenderType((ITypeDefOrRef)obj, g, bounds);
			else if (obj is PropertyDef)
				RenderProperty((PropertyDef)obj, g, bounds);
			else if (obj is EventDef)
				RenderEvent((EventDef)obj, g, bounds);
			else if (obj is MemberRef) {
				var memberRef = (MemberRef)obj;
				if (memberRef.IsFieldRef)
					RenderField(memberRef, g, bounds);
				else
					RenderMethod(memberRef, g, bounds);
			}
			else if (obj is IMethod)
				RenderMethod((IMethod)obj, g, bounds);
			else if (obj is IField)
				RenderField((IField)obj, g, bounds);
			else
				throw new NotSupportedException();
		}

		public static void RenderAssembly(IAssembly assembly, Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.assembly.png"), bounds);
		}

		public static void RenderModule(IModule module, Graphics g, Rectangle bounds) {
			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.module.png"), bounds);
		}

		public static void RenderType(ITypeDefOrRef type, Graphics g, Rectangle bounds) {
			var typeDef = type as TypeDef;
			if (typeDef == null) {
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.type.png"), bounds);
				return;
			}

			Image icon, visibility;

			icon = Resources.GetResource<Image>("Icons.ObjModel.type.png");
			if (typeDef.IsInterface) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.interface.png");
			}
			else if (typeDef.BaseType != null) {
				if (typeDef.IsEnum) {
					icon = Resources.GetResource<Image>("Icons.ObjModel.enum.png");
				}
				else if (typeDef.IsValueType && !typeDef.IsAbstract) {
					icon = Resources.GetResource<Image>("Icons.ObjModel.valuetype.png");
				}
				else if (typeDef.IsDelegate()) {
					icon = Resources.GetResource<Image>("Icons.ObjModel.delegate.png");
				}
			}

			switch (typeDef.Visibility) {
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

		public static void RenderMethod(IMethod method, Graphics g, Rectangle bounds) {
			var methodDef = method as MethodDef;
			if (methodDef == null) {
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.method.png"), bounds);
				return;
			}

			Image icon, visibility;

			icon = Resources.GetResource<Image>("Icons.ObjModel.method.png");
			if (methodDef.IsConstructor) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.constructor.png");
			}
			else if (methodDef.IsVirtual && !methodDef.IsAbstract) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.override.png");
			}

			switch (methodDef.Access) {
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
			if (methodDef.IsStatic)
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.static.png"), bounds);
		}

		public static void RenderField(IField field, Graphics g, Rectangle bounds) {
			var fieldDef = field as FieldDef;
			if (fieldDef == null) {
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.field.png"), bounds);
				return;
			}

			Image visibility;

			switch (fieldDef.Access) {
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

			if (fieldDef.IsStatic) {
				if (fieldDef.DeclaringType.IsEnum)
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

		public static void RenderProperty(PropertyDef property, Graphics g, Rectangle bounds) {
			Image icon, visibility;

			icon = Resources.GetResource<Image>("Icons.ObjModel.property.png");
			if (property.SetMethod == null) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.readonly.png");
			}
			else if (property.GetMethod == null) {
				icon = Resources.GetResource<Image>("Icons.ObjModel.writeonly.png");
			}

			switch (property.GetVisibility()) {
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
			if (property.GetAccessors().Any(m => m.IsStatic))
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.static.png"), bounds);
		}

		public static void RenderEvent(EventDef ev, Graphics g, Rectangle bounds) {
			Image visibility;

			switch (ev.GetVisibility()) {
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

			g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.event.png"), bounds);
			if (visibility != null)
				g.DrawImageUnscaledAndClipped(visibility, bounds);
			if (ev.GetAccessors().Any(m => m.IsStatic))
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.static.png"), bounds);
		}
	}
}