using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.IO;
using dnlib.PE;

namespace dnExplorer {
	public static class dnlibUtils {
		public static ImageSectionHeader ToImageSectionHeader(this IPEImage image, RVA rva) {
			foreach (var section in image.ImageSectionHeaders) {
				if (rva >= section.VirtualAddress &&
				    rva < section.VirtualAddress + Math.Max(section.VirtualSize, section.SizeOfRawData))
					return section;
			}
			return null;
		}

		public static IImageStream CreateStream(this IPEImage image, FileSection section) {
			return image.CreateStream(section.StartOffset, section.EndOffset - section.StartOffset);
		}

		public static string ToDescription(this MDToken token) {
			return string.Format("[{0} 0x{1:x}]", token.Table, token.Rid);
		}

		public static bool IsDelegate(this TypeDef type) {
			if (type.BaseType == null || !type.BaseType.DefinitionAssembly.IsCorLib())
				return false;

			string fullName = type.BaseType.FullName;
			return fullName == "System.Delegate" || fullName == "System.MulticastDelegate";
		}

		public static IEnumerable<MethodDef> GetMethods(this PropertyDef property) {
			if (property.GetMethod != null)
				yield return property.GetMethod;

			if (property.SetMethod != null)
				yield return property.SetMethod;

			foreach (var other in property.OtherMethods)
				yield return other;
		}

		public static IEnumerable<MethodDef> GetMethods(this EventDef evnt) {
			if (evnt.AddMethod != null)
				yield return evnt.AddMethod;

			if (evnt.RemoveMethod != null)
				yield return evnt.RemoveMethod;

			if (evnt.InvokeMethod != null)
				yield return evnt.InvokeMethod;

			foreach (var other in evnt.OtherMethods)
				yield return other;
		}

		public static MethodAttributes GetVisibility(this PropertyDef property) {
			var ret = MethodAttributes.CompilerControlled;

			foreach (var other in property.GetMethods()) {
				if (other.Access > ret)
					ret = other.Access;
			}

			return ret;
		}

		public static MethodAttributes GetVisibility(this EventDef evnt) {
			var ret = MethodAttributes.CompilerControlled;

			foreach (var other in evnt.GetMethods()) {
				if (other.Access > ret)
					ret = other.Access;
			}

			return ret;
		}
	}
}