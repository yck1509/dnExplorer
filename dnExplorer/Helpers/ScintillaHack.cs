using System;
using System.ComponentModel;
using System.Reflection;
using ScintillaNET;

namespace dnExplorer {
	public static class ScintillaHack {
		static readonly object key =
			typeof(Scintilla).GetField("_scNotificationEventKey", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

		static readonly PropertyInfo getEvents = typeof(Component).GetProperty("Events",
			BindingFlags.NonPublic | BindingFlags.Instance);

		public static void Apply(Scintilla instance) {
			var events = (EventHandlerList)getEvents.GetValue(instance, null);
			events[key] = null;
		}
	}
}