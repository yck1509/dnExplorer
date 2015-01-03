using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Res = WeifenLuo.WinFormsUI.Docking.Resources;

namespace dnExplorer.Theme {
	internal class VS2010Theme : ThemeBase {
		public static Color ARGB(uint color) {
			return Color.FromArgb((int)color);
		}

		public static Color Background = ARGB(0xFF293955);
		public static Brush BackgroundBrush = new SolidBrush(Background);

		public override void Apply(DockPanel dockPanel) {
			if (dockPanel == null) {
				throw new NullReferenceException("dockPanel");
			}

			Measures.SplitterSize = 6;
			dockPanel.Extender.DockPaneCaptionFactory = new VS2010PaneCaptionFactory();
			dockPanel.Extender.AutoHideStripFactory = new VS2010AutoHideStripFactory();
			dockPanel.Extender.AutoHideWindowFactory = new VS2010AutoHideWindowFactory();
			dockPanel.Extender.DockPaneStripFactory = new VS2010DockPaneStripFactory();
			dockPanel.Extender.DockPaneSplitterControlFactory = new VS2010DockPaneSplitterControlFactory();
			dockPanel.Extender.DockWindowSplitterControlFactory = new VS2010DockWindowSplitterControlFactory();
			dockPanel.Extender.DockWindowFactory = new VS2010DockWindowFactory();
			dockPanel.Extender.PaneIndicatorFactory = new VS2010PaneIndicatorFactory();
			dockPanel.Extender.PanelIndicatorFactory = new VS2010PanelIndicatorFactory();
			dockPanel.Extender.DockOutlineFactory = null;
			dockPanel.BackColor = Background;
			dockPanel.Padding = new Padding(6);
			dockPanel.Skin = CreateVisualStudio2010();

			dockPanel.MouseDown += (sender, e) => {
				dockPanel.ActiveAutoHideContent = null;
				dockPanel.Focus();
			};
			dockPanel.ActiveContentChanged += (sender, e) => {
				if (dockPanel.ActiveContent != null && dockPanel.ActiveAutoHideContent != null &&
				    dockPanel.ActiveContent != dockPanel.ActiveAutoHideContent)
					dockPanel.ActiveAutoHideContent = null;
			};
		}

		class VS2010AutoHideWindowFactory : DockPanelExtender.IAutoHideWindowFactory {
			public DockPanel.AutoHideWindowControl CreateAutoHideWindow(DockPanel panel) {
				return new VS2010AutoHideWindowControl(panel);
			}
		}

		class VS2010DockPaneSplitterControlFactory : DockPanelExtender.IDockPaneSplitterControlFactory {
			public DockPane.SplitterControlBase CreateSplitterControl(DockPane pane) {
				return new VS2010SplitterControl(pane);
			}
		}

		class VS2010DockWindowSplitterControlFactory : DockPanelExtender.IDockWindowSplitterControlFactory {
			public SplitterBase CreateSplitterControl() {
				return new VS2010DockWindow.VS2010DockWindowSplitterControl();
			}
		}

		class VS2010DockPaneStripFactory : DockPanelExtender.IDockPaneStripFactory {
			public DockPaneStripBase CreateDockPaneStrip(DockPane pane) {
				return new VS2010DockPaneStrip(pane);
			}
		}

		class VS2010AutoHideStripFactory : DockPanelExtender.IAutoHideStripFactory {
			public AutoHideStripBase CreateAutoHideStrip(DockPanel panel) {
				return new VS2010AutoHideStrip(panel);
			}
		}

		class VS2010PaneCaptionFactory : DockPanelExtender.IDockPaneCaptionFactory {
			public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane) {
				return new VS2010LightDockPaneCaption(pane);
			}
		}

		class VS2010DockWindowFactory : DockPanelExtender.IDockWindowFactory {
			public DockWindow CreateDockWindow(DockPanel dockPanel, DockState dockState) {
				return new VS2010DockWindow(dockPanel, dockState);
			}
		}

		class VS2010PanelIndicatorFactory : DockPanelExtender.IPanelIndicatorFactory {
			public DockPanel.IPanelIndicator CreatePanelIndicator(DockStyle style) {
				return new VS2010PanelIndicator(style);
			}

			class VS2010PanelIndicator : PictureBox, DockPanel.IPanelIndicator {
				static Image _imagePanelLeft = Indicators.DockIndicator_PanelLeft;
				static Image _imagePanelRight = Indicators.DockIndicator_PanelRight;
				static Image _imagePanelTop = Indicators.DockIndicator_PanelTop;
				static Image _imagePanelBottom = Indicators.DockIndicator_PanelBottom;
				static Image _imagePanelFill = Indicators.DockIndicator_PanelFill;
				static Image _imagePanelLeftActive = Indicators.DockIndicator_PanelLeft;
				static Image _imagePanelRightActive = Indicators.DockIndicator_PanelRight;
				static Image _imagePanelTopActive = Indicators.DockIndicator_PanelTop;
				static Image _imagePanelBottomActive = Indicators.DockIndicator_PanelBottom;
				static Image _imagePanelFillActive = Indicators.DockIndicator_PanelFill;

				public VS2010PanelIndicator(DockStyle dockStyle) {
					DockStyle = dockStyle;
					SizeMode = PictureBoxSizeMode.AutoSize;
					Image = ImageInactive;
				}

				DockStyle DockStyle { get; set; }

				DockStyle m_status;

				public DockStyle Status {
					get { return m_status; }
					set {
						if (value != DockStyle && value != DockStyle.None)
							throw new InvalidEnumArgumentException();

						if (m_status == value)
							return;

						m_status = value;
						IsActivated = (m_status != DockStyle.None);
					}
				}

				Image ImageInactive {
					get {
						if (DockStyle == DockStyle.Left)
							return _imagePanelLeft;
						if (DockStyle == DockStyle.Right)
							return _imagePanelRight;
						if (DockStyle == DockStyle.Top)
							return _imagePanelTop;
						if (DockStyle == DockStyle.Bottom)
							return _imagePanelBottom;
						if (DockStyle == DockStyle.Fill)
							return _imagePanelFill;
						return null;
					}
				}

				Image ImageActive {
					get {
						if (DockStyle == DockStyle.Left)
							return _imagePanelLeftActive;
						if (DockStyle == DockStyle.Right)
							return _imagePanelRightActive;
						if (DockStyle == DockStyle.Top)
							return _imagePanelTopActive;
						if (DockStyle == DockStyle.Bottom)
							return _imagePanelBottomActive;
						if (DockStyle == DockStyle.Fill)
							return _imagePanelFillActive;
						return null;
					}
				}

				bool m_isActivated;

				bool IsActivated {
					get { return m_isActivated; }
					set {
						m_isActivated = value;
						Image = IsActivated ? ImageActive : ImageInactive;
					}
				}

				public DockStyle HitTest(Point pt) {
					return Visible && ClientRectangle.Contains(PointToClient(pt)) ? DockStyle : DockStyle.None;
				}
			}
		}

		class VS2010PaneIndicatorFactory : DockPanelExtender.IPaneIndicatorFactory {
			public DockPanel.IPaneIndicator CreatePaneIndicator() {
				return new VS2010PaneIndicator();
			}

			class VS2010PaneIndicator : PictureBox, DockPanel.IPaneIndicator {
				static Bitmap _bitmapPaneDiamond = Indicators.Dockindicator_PaneDiamond;
				static Bitmap _bitmapPaneDiamondLeft = Indicators.Dockindicator_PaneDiamond_Fill;
				static Bitmap _bitmapPaneDiamondRight = Indicators.Dockindicator_PaneDiamond_Fill;
				static Bitmap _bitmapPaneDiamondTop = Indicators.Dockindicator_PaneDiamond_Fill;
				static Bitmap _bitmapPaneDiamondBottom = Indicators.Dockindicator_PaneDiamond_Fill;
				static Bitmap _bitmapPaneDiamondFill = Indicators.Dockindicator_PaneDiamond_Fill;
				static Bitmap _bitmapPaneDiamondHotSpot = Indicators.Dockindicator_PaneDiamond_Hotspot;
				static Bitmap _bitmapPaneDiamondHotSpotIndex = Res.DockIndicator_PaneDiamond_HotSpotIndex;

				static DockPanel.HotSpotIndex[] _hotSpots = {
					new DockPanel.HotSpotIndex(1, 0, DockStyle.Top),
					new DockPanel.HotSpotIndex(0, 1, DockStyle.Left),
					new DockPanel.HotSpotIndex(1, 1, DockStyle.Fill),
					new DockPanel.HotSpotIndex(2, 1, DockStyle.Right),
					new DockPanel.HotSpotIndex(1, 2, DockStyle.Bottom)
				};

				GraphicsPath _displayingGraphicsPath = DrawHelper.CalculateGraphicsPathFromBitmap(_bitmapPaneDiamond);

				public VS2010PaneIndicator() {
					SizeMode = PictureBoxSizeMode.AutoSize;
					Image = _bitmapPaneDiamond;
					Region = new Region(DisplayingGraphicsPath);
				}

				public GraphicsPath DisplayingGraphicsPath {
					get { return _displayingGraphicsPath; }
				}

				public DockStyle HitTest(Point pt) {
					if (!Visible)
						return DockStyle.None;

					pt = PointToClient(pt);
					if (!ClientRectangle.Contains(pt))
						return DockStyle.None;

					for (int i = _hotSpots.GetLowerBound(0); i <= _hotSpots.GetUpperBound(0); i++) {
						if (_bitmapPaneDiamondHotSpot.GetPixel(pt.X, pt.Y) ==
						    _bitmapPaneDiamondHotSpotIndex.GetPixel(_hotSpots[i].X, _hotSpots[i].Y))
							return _hotSpots[i].DockStyle;
					}

					return DockStyle.None;
				}

				DockStyle m_status = DockStyle.None;

				public DockStyle Status {
					get { return m_status; }
					set {
						m_status = value;
						if (m_status == DockStyle.None)
							Image = _bitmapPaneDiamond;
						else if (m_status == DockStyle.Left)
							Image = _bitmapPaneDiamondLeft;
						else if (m_status == DockStyle.Right)
							Image = _bitmapPaneDiamondRight;
						else if (m_status == DockStyle.Top)
							Image = _bitmapPaneDiamondTop;
						else if (m_status == DockStyle.Bottom)
							Image = _bitmapPaneDiamondBottom;
						else if (m_status == DockStyle.Fill)
							Image = _bitmapPaneDiamondFill;
					}
				}
			}
		}

		public static DockPanelSkin CreateVisualStudio2010() {
			var skin = new DockPanelSkin();
			skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.TextColor = Color.Black;
			skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.TextColor = Color.White;
			skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.TextColor = Color.Black;
			skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.TextColor = Color.White;
			skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.TextColor = Color.Black;
			skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.TextColor = Color.White;
			skin.AutoHideStripSkin.TabGradient.TextColor = Color.White;
			return skin;
		}
	}
}