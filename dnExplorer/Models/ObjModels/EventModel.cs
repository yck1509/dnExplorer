using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class EventModel : LazyModel, IHasInfo {
		public EventDef Event { get; set; }

		public EventModel(EventDef evnt) {
			Event = evnt;
		}

		protected override bool HasChildren {
			get {
				return Event.AddMethod != null || Event.RemoveMethod != null ||
				       Event.InvokeMethod != null || Event.HasOtherMethods;
			}
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var child in Event.GetMethods())
				yield return new MethodModel(child);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			Image visibility;

			switch (Event.GetVisibility()) {
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
			if (Event.GetMethods().Any(m => m.IsStatic))
				g.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.ObjModel.static.png"), bounds);
		}

		protected override void Refresh() {
			switch (Event.GetVisibility()) {
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
			Text = Utils.EscapeString(DisplayNameCreator.CreateDisplayName(Event), false);
		}

		string IHasInfo.Header {
			get { return Utils.EscapeString(Event.FullName, false); }
		}

		IEnumerable<KeyValuePair<string, string>> IHasInfo.GetInfos() {
			yield return
				new KeyValuePair<string, string>("Declaring Type", Utils.EscapeString(Event.DeclaringType.FullName, false));
			if (Event.DeclaringType.Scope != null)
				yield return
					new KeyValuePair<string, string>("Scope", Utils.EscapeString(Event.DeclaringType.Scope.ToString(), false));

			yield return new KeyValuePair<string, string>("Token", Event.MDToken.ToStringRaw());
			yield return new KeyValuePair<string, string>("Event Type", Utils.EscapeString(Event.EventType.FullName, false));
		}
	}
}