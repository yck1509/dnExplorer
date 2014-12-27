using System;
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
	}
}