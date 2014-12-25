using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace dnExplorer {
	internal static class Resources {
		static readonly Dictionary<string, object> cache = new Dictionary<string, object>();

		public static T GetResource<T>(string name) where T : class {
			if (cache.ContainsKey(name)) {
				return (cache[name] as T);
			}
			var res = typeof(Resources).Assembly.GetManifestResourceStream("dnExplorer.Resources." + name);
			if (typeof(T) == typeof(Stream)) {
				return res as T;
			}
			if (typeof(T) == typeof(string)) {
				return new StreamReader(res).ReadToEnd() as T;
			}
			if (typeof(T) == typeof(Image)) {
				return Image.FromStream(res) as T;
			}
			if (typeof(T) == typeof(Icon)) {
				return new Icon(res) as T;
			}
			throw new NotSupportedException();
		}
	}
}