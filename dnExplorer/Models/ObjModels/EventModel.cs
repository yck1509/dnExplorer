using System;
using System.Collections.Generic;
using System.Drawing;
using dnExplorer.Trees;
using dnlib.DotNet;

namespace dnExplorer.Models {
	public class EventModel : ObjModel, IHasInfo {
		public EventDef Event { get; set; }

		public override IDnlibDef Definition {
			get { return Event; }
		}

		public EventModel(EventDef evnt) {
			Event = evnt;
		}

		protected override bool HasChildren {
			get { return !Event.IsEmpty; }
		}

		protected override bool IsVolatile {
			get { return false; }
		}

		protected override IEnumerable<IDataModel> PopulateChildren() {
			foreach (var child in Event.GetAccessors())
				yield return new MethodModel(child);
		}

		public override bool HasIcon {
			get { return true; }
		}

		public override void DrawIcon(Graphics g, Rectangle bounds) {
			ObjectIconRenderer.RenderEvent(Event, g, bounds);
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
			get { return Utils.EscapeString(DisplayNameCreator.CreateFullName(Event), false); }
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