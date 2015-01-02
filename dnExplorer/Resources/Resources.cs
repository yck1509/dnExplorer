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
			var res = typeof(Resources).Assembly.GetManifestResourceStream(Main.AppName + ".Resources." + name);
			T ret;
			if (typeof(T) == typeof(Stream)) {
				ret = res as T;
			}
			else if (typeof(T) == typeof(string)) {
				ret = new StreamReader(res).ReadToEnd() as T;
			}
			else if (typeof(T) == typeof(Image)) {
				ret = Image.FromStream(res) as T;
			}
			else if (typeof(T) == typeof(Icon)) {
				ret = new Icon(res) as T;
			}
			else
				throw new NotSupportedException();

			cache[name] = ret;
			return ret;
		}
	}
}