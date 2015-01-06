using System;
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
		}

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
		}

		CodeViewData data;

		public void SetData(CodeViewData data) {
			this.data = data;
			IsReadOnly = false;
			Text = data.Code;
			IsReadOnly = true;
		}

		protected override void OnStyleNeeded(StyleNeededEventArgs e) {
			base.OnStyleNeeded(e);

			for (int i = e.Range.Start; i < e.Range.End; i++) {
				CodeViewData.TextType type;
				if (data.Types.TryGetValue(i, out type))
					GetRange(i, i + type.Length).SetStyle(type.Type);
			}
		}
	}
}