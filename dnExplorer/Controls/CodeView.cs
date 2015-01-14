using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ScintillaNET;

namespace dnExplorer.Controls {
	public class CodeView : Scintilla {
		public CodeView() {
			Dock = DockStyle.Fill;
			BackColor = Color.FromArgb(0xff, 0xff, 0xe1);
			Folding.IsEnabled = false;
			Font = new Font("Consolas", 9);
			IsReadOnly = true;
			Scrolling.HorizontalScrollTracking = true;
			Indentation.TabWidth = 4;

			SetupStyles();
			ScintillaHack.Apply(this);

			SelectionChanged += (sender, e) => UpdateIndicator();
			Scroll += (sender, e) => UpdateIndicator();
		}

		const int INDI_REFERENCE = 1;

		void SetupStyles() {
			Styles.Default.Font = Font;
			Styles.Default.BackColor = BackColor;
			Styles.Default.ForeColor = Color.FromArgb(0x00, 0x00, 0x00);
			Styles.ClearAll();

			Styles[CodeViewData.TYPE_COMMENT].ForeColor = Color.FromArgb(0x80, 0x80, 0x80);

			Styles[CodeViewData.TYPE_DEF].ForeColor = Color.FromArgb(0x00, 0x00, 0x00);
			Styles[CodeViewData.TYPE_DEF].Bold = true;

			Styles[CodeViewData.TYPE_DEF_TARGET].ForeColor = Color.FromArgb(0x00, 0x00, 0x00);
			Styles[CodeViewData.TYPE_DEF_TARGET].Bold = true;
			Styles[CodeViewData.TYPE_DEF_TARGET].IsHotspot = true;

			Styles[CodeViewData.TYPE_KEYWORD].ForeColor = Color.FromArgb(0x00, 0x00, 0x80);

			Styles[CodeViewData.TYPE_LITERAL].ForeColor = Color.FromArgb(0x80, 0x00, 0x00);

			Styles[CodeViewData.TYPE_REF].ForeColor = Color.FromArgb(0x00, 0x80, 0x00);

			Styles[CodeViewData.TYPE_REF_TARGET].ForeColor = Color.FromArgb(0x00, 0x80, 0x00);
			Styles[CodeViewData.TYPE_REF_TARGET].IsHotspot = true;

			// UseActiveXXX is only updated when corresponding ActiveXXX is updated.
			HotspotStyle.UseActiveBackColor = false;
			HotspotStyle.ActiveBackColor = BackColor;
			HotspotStyle.UseActiveForeColor = false;
			HotspotStyle.ActiveForeColor = ForeColor;

			Indicators[INDI_REFERENCE].Style = IndicatorStyle.RoundBox;
			Indicators[INDI_REFERENCE].Color = Color.Red;
			Indicators[INDI_REFERENCE].Alpha = 64;
			Indicators[INDI_REFERENCE].OutlineAlpha = 64;
			Indicators[INDI_REFERENCE].DrawMode = IndicatorDrawMode.Underlay;
		}

		CodeViewData data;
		public CodeViewData Data { get { return data; } }

		public void Clear() {
			SetPlainText("");
		}

		public void SetPlainText(string text) {
			data = null;
			IsReadOnly = false;
			Text = text;
			IsReadOnly = true;
		}

		public void SetData(CodeViewData data) {
			this.data = data;
			IsReadOnly = false;
			Text = data.Code;
			IsReadOnly = true;
		}

		protected override void OnStyleNeeded(StyleNeededEventArgs e) {
			base.OnStyleNeeded(e);

			if (data == null)
				return;

			for (int i = e.Range.Start; i < e.Range.End; i++) {
				CodeViewData.TextType type;
				if (data.Types.TryGetValue(i, out type))
					GetRange(i, i + type.Length).SetStyle(type.Type);
			}
		}

		CodeViewData.TextRef? ResolveReference(ref int pos) {
			if (data == null)
				return null;

			// Assuming no reference ranges overlaps
			int target = pos;
			for (; pos >= 0; pos--) {
				CodeViewData.TextRef textRef;
				if (data.References.TryGetValue(pos, out textRef)) {
					if (pos + textRef.Length > target)
						return textRef;
					return null;
				}
			}
			return null;
		}

		void UpdateIndicator() {
			if (data == null)
				return;

			foreach (Range r in Indicators[INDI_REFERENCE].SearchAll())
				r.ClearIndicator(INDI_REFERENCE);

			var sel = Selection.End;
			var target = ResolveReference(ref sel);
			if (target != null && target.Value.IsLocal) {
				int visibleBegin = PositionFromPoint(0, 0);
				int visibleEnd = PositionFromPoint(Width - 1, Height - 1);

				for (int i = visibleBegin; i < visibleEnd; i++) {
					CodeViewData.TextRef textRef;
					if (data.References.TryGetValue(i, out textRef) &&
					    textRef.Reference.Equals(target.Value.Reference)) {
						GetRange(i, i + textRef.Length).SetIndicator(INDI_REFERENCE);
					}
				}
			}
		}

		protected override void OnHotspotDoubleClick(HotspotClickEventArgs e) {
			base.OnHotspotDoubleClick(e);
			int pos = e.Position;
			var textRef = ResolveReference(ref pos);
			Debug.Assert(textRef != null);
			if (Navigate != null) {
				var r = textRef.Value;
				Navigate(this, new CodeViewNavigateEventArgs(r.IsLocal, r.IsDefinition, r.Reference));
			}
		}

		public event EventHandler<CodeViewNavigateEventArgs> Navigate;
	}

	public class CodeViewNavigateEventArgs : EventArgs {
		public bool IsLocal { get; private set; }
		public bool IsDefinition { get; private set; }
		public object Target { get; private set; }

		internal CodeViewNavigateEventArgs(bool isLocal, bool isDef, object target) {
			IsLocal = isLocal;
			IsDefinition = isDef;
			Target = target;
		}
	}
}