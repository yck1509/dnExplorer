using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace dnExplorer {
	internal class IconCreator {
		[DllImport("user32.dll")]
		static extern bool DestroyIcon(IntPtr handle);

		public static Icon CreateIcon(Bitmap bmp, int size) {
			var hIcon = bmp.GetHicon();
			var icon = Icon.FromHandle(hIcon);
			icon = new Icon(icon, size, size);
			DestroyIcon(hIcon);
			return icon;
		}
	}
}