using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer.Theme {
	[ToolboxItem(false)]
	internal class VS2010DockWindow : DockWindow {
		public VS2010DockWindow(DockPanel dockPanel, DockState dockState) : base(dockPanel, dockState) {
		}

		public override Rectangle DisplayingRectangle {
			get {
				Rectangle rect = ClientRectangle;
				if (DockState == DockState.DockLeft)
					rect.Width -= Measures.SplitterSize;
				else if (DockState == DockState.DockRight) {
					rect.X += Measures.SplitterSize;
					rect.Width -= Measures.SplitterSize;
				}
				else if (DockState == DockState.DockTop)
					rect.Height -= Measures.SplitterSize;
				else if (DockState == DockState.DockBottom) {
					rect.Y += Measures.SplitterSize;
					rect.Height -= Measures.SplitterSize;
				}

				return rect;
			}
		}

		internal class VS2010DockWindowSplitterControl : SplitterBase {
			protected override int SplitterSize {
				get { return Measures.SplitterSize; }
			}

			protected override void StartDrag() {
				DockWindow window = Parent as DockWindow;
				if (window == null)
					return;

				window.DockPanel.BeginDrag(window, window.RectangleToScreen(Bounds));
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
}