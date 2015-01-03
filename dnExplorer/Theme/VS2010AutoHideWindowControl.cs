using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer.Theme {
	internal class VS2010AutoHideWindowControl : DockPanel.AutoHideWindowControl {
		static int SplitterSize {
			get { return 3; }
		}

		class VS2010AutoHideWindowSplitterControl : SplitterBase {
			static readonly SolidBrush brush = new SolidBrush(VS2010Theme.ARGB(0xFF293955));

			public VS2010AutoHideWindowSplitterControl(DockPanel.AutoHideWindowControl autoHideWindow) {
				AutoHideWindow = autoHideWindow;
			}

			DockPanel.AutoHideWindowControl AutoHideWindow { get; set; }

			protected override int SplitterSize {
				get { return VS2010AutoHideWindowControl.SplitterSize; }
			}

			protected override void StartDrag() {
				AutoHideWindow.DockPanel.BeginDrag(AutoHideWindow, AutoHideWindow.RectangleToScreen(Bounds));
			}

			protected override void OnPaint(PaintEventArgs e) {
				base.OnPaint(e);

				Rectangle rect = ClientRectangle;

				if (rect.Width <= 0 || rect.Height <= 0)
					return;

				e.Graphics.FillRectangle(brush, rect);
			}
		}

		public VS2010AutoHideWindowControl(DockPanel dockPanel)
			: base(dockPanel) {
			m_splitter = new VS2010AutoHideWindowSplitterControl(this);
			Controls.Add(m_splitter);
		}

		protected override Rectangle DisplayingRectangle {
			get {
				Rectangle rect = ClientRectangle;

				if (DockState == DockState.DockLeftAutoHide)
					rect.Width -= SplitterSize;
				else if (DockState == DockState.DockRightAutoHide) {
					rect.X += SplitterSize;
					rect.Width -= SplitterSize;
				}
				else if (DockState == DockState.DockTopAutoHide)
					rect.Height -= SplitterSize;
				else if (DockState == DockState.DockBottomAutoHide) {
					rect.Y += SplitterSize;
					rect.Height -= SplitterSize;
				}

				return rect;
			}
		}

		protected override void OnLayout(LayoutEventArgs levent) {
			DockPadding.All = 0;
			if (DockState == DockState.DockLeftAutoHide) {
				//DockPadding.Right = 2;
				m_splitter.Dock = DockStyle.Right;
			}
			else if (DockState == DockState.DockRightAutoHide) {
				//DockPadding.Left = 2;
				m_splitter.Dock = DockStyle.Left;
			}
			else if (DockState == DockState.DockTopAutoHide) {
				//DockPadding.Bottom = 2;
				m_splitter.Dock = DockStyle.Bottom;
			}
			else if (DockState == DockState.DockBottomAutoHide) {
				//DockPadding.Top = 2;
				m_splitter.Dock = DockStyle.Top;
			}

			Rectangle rectDisplaying = DisplayingRectangle;
			Rectangle rectHidden = new Rectangle(-rectDisplaying.Width, rectDisplaying.Y, rectDisplaying.Width,
				rectDisplaying.Height);
			foreach (Control c in Controls) {
				DockPane pane = c as DockPane;
				if (pane == null)
					continue;


				if (pane == ActivePane)
					pane.Bounds = rectDisplaying;
				else
					pane.Bounds = rectHidden;
			}

			base.OnLayout(levent);
		}
	}
}