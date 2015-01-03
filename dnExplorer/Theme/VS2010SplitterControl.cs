using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer.Theme {
	internal class VS2010SplitterControl : DockPane.SplitterControlBase {
		public VS2010SplitterControl(DockPane pane)
			: base(pane) {
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);

			Rectangle rect = ClientRectangle;

			if (rect.Width <= 0 || rect.Height <= 0)
				return;

			e.Graphics.FillRectangle(VS2010Theme.BackgroundBrush, rect);
		}
	}
}